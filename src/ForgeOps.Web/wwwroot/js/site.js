
"use strict";

const statusLabel = value => value === "InProgress" ? "In progress" : value;
const facilityTime = value => new Intl.DateTimeFormat("en-CA", {
    month: "short", day: "numeric", hour: "numeric", minute: "2-digit",
    hour12: true, timeZone: "America/Halifax"
}).format(new Date(value));

function showFeedback(element, message, success) {
    element.textContent = message;
    element.className = success ? "notice notice-success" : "notice notice-error";
    element.hidden = false;
}

const statusForm = document.querySelector("#status-form");
if (statusForm) {
    statusForm.addEventListener("submit", async event => {
        event.preventDefault();
        const button = statusForm.querySelector("button[type=submit]");
        const feedback = document.querySelector("#status-feedback");
        button.disabled = true;
        feedback.hidden = true;
        try {
            const response = await fetch("/api/workorders/" + statusForm.dataset.orderId + "/status", {
                method: "PATCH",
                headers: { "Content-Type": "application/json", "Accept": "application/json" },
                body: JSON.stringify({
                    status: document.querySelector("#status-select").value,
                    resolutionNotes: document.querySelector("#status-notes").value
                })
            });
            const result = await response.json();
            if (!response.ok) {
                const errors = result.errors ? Object.values(result.errors).flat().join(" ") : result.title;
                showFeedback(feedback, errors || "The status could not be saved.", false);
                return;
            }
            const badge = document.querySelector("#status-badge");
            badge.textContent = statusLabel(result.status);
            badge.className = "badge-status status-" + result.status.toLowerCase();
            document.querySelector("#status-notes").value = result.resolutionNotes || "";
            document.querySelector("#resolution-display").textContent = result.resolutionNotes || "";
            document.querySelector("#resolution-block").hidden = !result.resolutionNotes;
            document.querySelector("#overdue-label").hidden = !result.isOverdue;
            document.querySelector("#due-date-label").classList.toggle("due-overdue", result.isOverdue);
            document.querySelector("#updated-label").textContent = facilityTime(result.updatedAtUtc);
            const history = document.querySelector("#history-list");
            const entries = result.history.map(entry => {
                const item = document.createElement("li");
                const dot = document.createElement("span");
                dot.className = "history-dot";
                const content = document.createElement("div");
                const summary = document.createElement("strong");
                summary.textContent = entry.summary;
                const time = document.createElement("time");
                time.dateTime = entry.occurredAtUtc;
                time.textContent = facilityTime(entry.occurredAtUtc);
                content.append(summary, time);
                if (entry.notes) {
                    const notes = document.createElement("p");
                    notes.className = "history-notes preserve-lines";
                    notes.textContent = entry.notes;
                    content.append(notes);
                }
                item.append(dot, content);
                return item;
            });
            history.replaceChildren(...entries);
            document.querySelector("#history-count").textContent = result.history.length;
            showFeedback(feedback, "Status saved. The operational dashboard will reflect this change.", true);
        } catch {
            showFeedback(feedback, "Could not reach ForgeOps. Check the connection and try again.", false);
        } finally {
            button.disabled = false;
        }
    });
}

const refresh = document.querySelector("#refresh-metrics");
if (refresh) {
    refresh.addEventListener("click", async () => {
        const feedback = document.querySelector("#refresh-feedback");
        refresh.disabled = true;
        try {
            const response = await fetch("/api/dashboard/summary", { headers: { "Accept": "application/json" }, cache: "no-store" });
            if (!response.ok) throw new Error("Request failed");
            const result = await response.json();
            document.querySelectorAll("[data-metric]").forEach(element => {
                element.textContent = result[element.dataset.metric];
            });
            document.querySelectorAll("[data-status-count]").forEach(element => {
                element.textContent = result.byStatus[element.dataset.statusCount];
            });
            document.querySelectorAll("[data-status-bar]").forEach(element => {
                element.style.width = (result.total ? result.byStatus[element.dataset.statusBar] * 100 / result.total : 0) + "%";
            });
            document.querySelectorAll("[data-priority-count]").forEach(element => {
                element.textContent = result.byPriority[element.dataset.priorityCount];
            });
            document.querySelector("#overdue-notice").hidden = result.overdue === 0;
            document.querySelector("#operational-date").textContent = new Intl.DateTimeFormat("en-CA", {
                weekday: "long", month: "long", day: "numeric", year: "numeric"
            }).format(new Date(result.today + "T12:00:00"));
            feedback.textContent = "Metrics refreshed just now";
        } catch {
            feedback.textContent = "Could not refresh metrics. Please try again.";
        } finally {
            refresh.disabled = false;
        }
    });
}

const DEFAULT_STACK_LIMIT = 1000000;

const ITEM_DEFS = {
    chip_green: {
        name: "Green Chips",
        icon: "assets/icons/chip_green_dark.png",
        description: "Lucky green tokens earned from scratchers.",
        price: 2,
        stackLimit: DEFAULT_STACK_LIMIT,
        stackable: true,
        unique: false
    },
    chip_blue: {
        name: "Blue Chips",
        icon: "assets/icons/chip_blue_dark.png",
        description: "Rare keno chips with higher exchange value.",
        price: 5,
        stackLimit: DEFAULT_STACK_LIMIT,
        stackable: true,
        unique: false
    }
};

const CURRENCY_DEFS = {
    cash: { name: "Cash", icon: "assets/icons/cash_dark.png" },
    credit: { name: "Credit", icon: "assets/icons/credit_dark.png" }
};

const state = {
    tickRate: 1,
    tick: 0,
    isLocked: false,
    sandboxEnabled: true,
    zoom: 1,
    wallet: { cash: 125, credit: 40 },
    inventorySlots: [],
    inventoryHistory: {},
    skills: [
        {
            id: "gambling",
            name: "Gambling",
            icon: "🎰",
            iconImage: "assets/icons/slots_dark.png",
            level: 1,
            xp: 0,
            maxLevel: 60,
            active: true,
            task: "scratchers",
            tasks: [
                {
                    id: "scratchers",
                    label: "Scratchers",
                    level: 1,
                    reward: "1-3 Green Chips",
                    rewardItemId: "chip_green",
                    iconImage: "assets/icons/scratcher_dark.png"
                },
                {
                    id: "keno",
                    label: "Keno games",
                    level: 5,
                    reward: "1-3 Blue Chips",
                    rewardItemId: "chip_blue",
                    iconImage: "assets/icons/keno_dark.png"
                }
            ]
        },
        {
            id: "idling",
            name: "Idling",
            icon: "✨",
            iconImage: "assets/icons/afk.png",
            level: 1,
            xp: 0,
            maxLevel: 60,
            active: false,
            task: "idle",
            tasks: [
                { id: "idle", label: "Idle", level: 1, reward: "Rest and recover" },
                { id: "flip", label: "Flip a coin", level: 5, reward: "Clear your mind" },
                { id: "daydream", label: "Daydream", level: 10, reward: "Wander thoughts" },
                { id: "nap", label: "Power nap", level: 15, reward: "Recharge" },
                { id: "lucid", label: "Lucid drift", level: 20, reward: "Deep focus" }
            ]
        }
    ],
    world: ["Universe: Meadow", "World: Bristlewood", "Region: North Reach", "Zone: Clearing", "Node: Camp"],
    equipment: ["Starter Deck", "Lucky Token"],
    quests: ["Win Scratchers (0/5)", "Play Keno (0/3)"],
    achievements: ["High Roller (0/10)", "Stack Chips (0/12)"],
    collections: ["Gambling Set: 1/3"],
    combat: ["player hits slime (3)", "slime misses"],
    crafting: ["Redeem Chips x1 (2 green)", "Lucky Charm x1 (1 blue)"],
    trade: ["3 Green Chips -> 2 Cash"],
    generator: ["Biome: Casino", "Weather: Neon"],
    compendium: ["Green Chips", "Blue Chips"],
    saveStatus: "Ready"
};

const ui = {
    saveStatus: document.getElementById("saveStatus"),
    worldList: document.getElementById("worldList"),
    walletList: document.getElementById("walletList"),
    skillsList: document.getElementById("skillsList"),
    skillIcon: document.getElementById("skillIcon"),
    skillIconImg: document.getElementById("skillIconImg"),
    skillName: document.getElementById("skillName"),
    skillDetails: document.getElementById("skillDetails"),
    skillLevel: document.getElementById("skillLevel"),
    taskList: document.getElementById("taskList"),
    taskAction: document.getElementById("taskAction"),
    taskActionHint: document.getElementById("taskActionHint"),
    inventoryList: document.getElementById("inventoryList"),
    inventoryPopover: document.getElementById("inventoryPopover"),
    popoverIcon: document.getElementById("popoverIcon"),
    popoverName: document.getElementById("popoverName"),
    popoverDesc: document.getElementById("popoverDesc"),
    popoverStack: document.getElementById("popoverStack"),
    popoverValue: document.getElementById("popoverValue"),
    popoverGraph: document.getElementById("popoverGraph"),
    equipmentList: document.getElementById("equipmentList"),
    questList: document.getElementById("questList"),
    achievementList: document.getElementById("achievementList"),
    collectionList: document.getElementById("collectionList"),
    combatList: document.getElementById("combatList"),
    craftingList: document.getElementById("craftingList"),
    tradeList: document.getElementById("tradeList"),
    generatorList: document.getElementById("generatorList"),
    compendiumList: document.getElementById("compendiumList"),
    contextRing: document.getElementById("contextRing"),
    contextRingSvg: document.getElementById("contextRingSvg"),
    contextRingProgress: document.getElementById("contextRingProgress"),
    contextRingTip: document.getElementById("contextRingTip"),
    contextRingParticles: document.getElementById("contextRingParticles"),
    contextRingWrap: document.querySelector(".ring-wrap"),
    debugOverlay: document.getElementById("debugOverlay"),
    debugTickRate: document.getElementById("debugTickRate"),
    debugFps: document.getElementById("debugFps"),
    debugUiUpdates: document.getElementById("debugUiUpdates"),
    debugOnline: document.getElementById("debugOnline"),
    debugOffline: document.getElementById("debugOffline"),
    walletInspector: document.getElementById("walletInspector"),
    inventoryInspector: document.getElementById("inventoryInspector"),
    skillInspector: document.getElementById("skillInspector"),
    zoomSlider: document.getElementById("zoomSlider"),
    zoomValue: document.getElementById("zoomValue")
};

let selectedSkillId = "idling";
let pendingTaskBounceId = null;
let running = true;
let lastTick = performance.now();
let fpsFrames = 0;
let fpsElapsed = 0;
let uiUpdates = 0;
let lastUiUpdateCount = 0;
let lastOfflineSeconds = 0;
let lastParticleTime = 0;
let lastCompletionBurst = 0;
let popoverPinned = false;
let pendingIconBurstAt = 0;
let pendingIconBurstSkillId = null;
let iconComboLevel = 0;
let iconComboExpireAt = 0;
let iconBurstTimeoutId = null;

const STORAGE_KEY = "idle-sdk-web-demo-state";
const RING_RADIUS = 38;
const RING_CIRCUMFERENCE = 2 * Math.PI * RING_RADIUS;

const skillUi = new Map();
const skillProgress = new Map();
const skillCompletionHold = new Map();
const contextCard = document.querySelector(".skill-context");

function clamp(value, min, max) {
    return Math.min(max, Math.max(min, value));
}

function randInt(min, max) {
    return Math.floor(Math.random() * (max - min + 1)) + min;
}

function formatCompactNumber(value) {
    const abs = Math.abs(value);
    if (abs >= 1e12) return `${(value / 1e12).toFixed(2).replace(/\.00$/, "")}T`;
    if (abs >= 1e9) return `${(value / 1e9).toFixed(2).replace(/\.00$/, "")}B`;
    if (abs >= 1e6) return `${(value / 1e6).toFixed(2).replace(/\.00$/, "")}M`;
    if (abs >= 1e3) return `${(value / 1e3).toFixed(2).replace(/\.00$/, "")}K`;
    return `${value}`;
}

function getItemDef(itemId) {
    return ITEM_DEFS[itemId] ?? {
        name: itemId,
        icon: null,
        description: "",
        price: 0,
        stackLimit: DEFAULT_STACK_LIMIT,
        stackable: true,
        unique: false
    };
}

function getStackLimit(def) {
    if (def.unique || def.stackable === false) return 1;
    return def.stackLimit ?? DEFAULT_STACK_LIMIT;
}

function getInventoryTotals() {
    const totals = {};
    state.inventorySlots.forEach((slot) => {
        totals[slot.id] = (totals[slot.id] ?? 0) + slot.quantity;
    });
    return totals;
}

function addItemToInventory(itemId, quantity) {
    if (quantity <= 0) return;
    const def = getItemDef(itemId);
    const stackLimit = getStackLimit(def);
    const remainingSlots = [...state.inventorySlots];

    if (def.unique) {
        const alreadyOwned = remainingSlots.some((slot) => slot.id === itemId && slot.quantity > 0);
        if (!alreadyOwned) {
            state.inventorySlots.push({ id: itemId, quantity: 1 });
        }
        return;
    }

    let remaining = quantity;

    if (def.stackable !== false) {
        state.inventorySlots.forEach((slot) => {
            if (slot.id !== itemId) return;
            if (slot.quantity >= stackLimit) return;
            const space = stackLimit - slot.quantity;
            const toAdd = Math.min(space, remaining);
            if (toAdd > 0) {
                slot.quantity += toAdd;
                remaining -= toAdd;
            }
        });
    }

    while (remaining > 0) {
        const toAdd = def.stackable === false ? 1 : Math.min(stackLimit, remaining);
        state.inventorySlots.push({ id: itemId, quantity: toAdd });
        remaining -= toAdd;
    }

    const totals = getInventoryTotals();
    const history = state.inventoryHistory[itemId] ?? [];
    history.push({ t: Date.now(), v: totals[itemId] ?? 0 });
    state.inventoryHistory[itemId] = history.slice(-50);
}

function removeItemFromInventory(itemId, quantity) {
    if (quantity <= 0) return;
    let remaining = quantity;
    for (let i = state.inventorySlots.length - 1; i >= 0 && remaining > 0; i -= 1) {
        const slot = state.inventorySlots[i];
        if (slot.id !== itemId) continue;
        const toRemove = Math.min(slot.quantity, remaining);
        slot.quantity -= toRemove;
        remaining -= toRemove;
        if (slot.quantity <= 0) {
            state.inventorySlots.splice(i, 1);
        }
    }
}

function showInventoryPopover(slot, anchor) {
    if (!ui.inventoryPopover || !ui.popoverIcon || !ui.popoverName || !ui.popoverDesc || !ui.popoverStack || !ui.popoverValue || !ui.popoverGraph) {
        return;
    }
    const meta = getItemDef(slot.id);
    const stackLimit = getStackLimit(meta);
    const cashIcon = CURRENCY_DEFS.cash?.icon ?? "";
    const unitValue = meta.price ?? 0;
    const totalValue = unitValue * slot.quantity;
    ui.popoverIcon.src = meta.icon ?? "";
    ui.popoverIcon.alt = meta.name;
    ui.popoverName.textContent = meta.name;
    ui.popoverDesc.textContent = meta.description || "No description.";
    ui.popoverStack.innerHTML = `
        <div class="inventory-popover-label">STACK</div>
        <div class="inventory-popover-value-line stack-qty">${slot.quantity.toLocaleString()}</div>
        <div class="inventory-popover-value-line stack-max">MAX ${stackLimit.toLocaleString()}</div>
    `;
    ui.popoverValue.innerHTML = `
        <span class="inventory-popover-value">
            <span class="inventory-popover-label">EACH</span>
            <img src="${cashIcon}" alt="Cash" class="inventory-popover-cash" /> ${unitValue.toLocaleString()}
        </span>
        <span class="inventory-popover-value">
            <span class="inventory-popover-label">TOTAL</span>
            <img src="${cashIcon}" alt="Cash" class="inventory-popover-cash" /> ${totalValue.toLocaleString()}
        </span>
    `;

    drawInventoryGraph(slot.id);

    const anchorRect = anchor.getBoundingClientRect();
    ui.inventoryPopover.style.left = `${anchorRect.right + 12 + window.scrollX}px`;
    ui.inventoryPopover.style.top = `${anchorRect.top + window.scrollY}px`;
    ui.inventoryPopover.classList.remove("hidden");
    ui.inventoryPopover.setAttribute("aria-hidden", "false");
}

function hideInventoryPopover() {
    if (!ui.inventoryPopover) return;
    ui.inventoryPopover.classList.add("hidden");
    ui.inventoryPopover.setAttribute("aria-hidden", "true");
}

function drawInventoryGraph(itemId) {
    const canvas = ui.popoverGraph;
    if (!canvas) return;
    const ctx = canvas.getContext("2d");
    if (!ctx) return;
    const history = state.inventoryHistory[itemId] ?? [];
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    if (history.length < 2) {
        ctx.strokeStyle = "rgba(148, 163, 184, 0.5)";
        ctx.beginPath();
        ctx.moveTo(0, canvas.height - 4);
        ctx.lineTo(canvas.width, canvas.height - 4);
        ctx.stroke();
        return;
    }
    const values = history.map((point) => point.v);
    const min = Math.min(...values);
    const max = Math.max(...values);
    const span = Math.max(1, max - min);
    ctx.strokeStyle = "rgba(183, 247, 255, 0.85)";
    ctx.lineWidth = 2;
    ctx.beginPath();
    values.forEach((value, index) => {
        const x = (index / (values.length - 1)) * (canvas.width - 8) + 4;
        const y = canvas.height - 6 - ((value - min) / span) * (canvas.height - 12);
        if (index === 0) {
            ctx.moveTo(x, y);
        } else {
            ctx.lineTo(x, y);
        }
    });
    ctx.stroke();
}

function createListItem(text) {
    const li = document.createElement("li");
    li.textContent = text;
    return li;
}

function xpForLevel(level) {
    if (level <= 1) return 0;
    let total = 0;
    const base = 60;
    const growth = 1.5;
    for (let i = 2; i <= level; i++) {
        total += base * Math.pow(growth, i - 2);
    }
    return Math.round(total);
}

function getProgress(skill) {
    const current = xpForLevel(skill.level);
    const next = skill.level >= skill.maxLevel ? current : xpForLevel(skill.level + 1);
    if (next === current) return 1;
    return clamp((skill.xp - current) / (next - current), 0, 1);
}

function renderList(listEl, values) {
    listEl.innerHTML = "";
    values.forEach((line) => listEl.appendChild(createListItem(line)));
}

function renderWallet() {
    ui.walletList.innerHTML = "";
    Object.entries(state.wallet).forEach(([id, amount]) => {
        const currency = CURRENCY_DEFS[id] ?? { name: id, icon: null };
        const li = document.createElement("li");
        li.className = "wallet-row";
        const iconMarkup = currency.icon
            ? `<img src="${currency.icon}" alt="${currency.name}" class="item-icon" />`
            : `<span class="item-icon">${id.slice(0, 1).toUpperCase()}</span>`;
        li.innerHTML = `${iconMarkup}<span>${currency.name}</span><span class="subtle">${amount}</span>`;
        ui.walletList.appendChild(li);
    });
}

function renderInventory() {
    ui.inventoryList.innerHTML = "";
    ui.inventoryList.className = "list inventory-grid";
    if (state.inventorySlots.length === 0) {
        const li = document.createElement("li");
        li.className = "inventory-empty";
        li.textContent = "No items yet. Start a task to collect rewards.";
        ui.inventoryList.appendChild(li);
        return;
    }

    state.inventorySlots.forEach((slot) => {
        const meta = getItemDef(slot.id);
        const stackLimit = getStackLimit(meta);
        const li = document.createElement("li");
        li.className = "inventory-slot";
        li.innerHTML = `
            <div class="inventory-badge" data-item="${slot.id}" data-qty="${slot.quantity}" data-stack="${stackLimit}">
                <div class="inventory-icon-wrap">
                    <img src="${meta.icon}" alt="${meta.name}" class="inventory-icon" />
                </div>
                <div class="inventory-amount">${formatCompactNumber(slot.quantity)}</div>
            </div>
        `;
        const badge = li.querySelector(".inventory-badge");
        if (badge) {
            badge.addEventListener("mouseenter", () => showInventoryPopover(slot, badge));
            badge.addEventListener("mouseleave", () => {
                if (!popoverPinned) hideInventoryPopover();
            });
            badge.addEventListener("click", (event) => {
                event.stopPropagation();
                popoverPinned = !popoverPinned;
                if (popoverPinned) {
                    showInventoryPopover(slot, badge);
                } else {
                    hideInventoryPopover();
                }
            });
        }
        ui.inventoryList.appendChild(li);
    });
}

function renderSkills() {
    ui.skillsList.innerHTML = "";
    skillUi.clear();
    skillProgress.clear();
    state.skills.forEach((skill) => {
        const li = document.createElement("li");
        li.className = "skill-row";
        const dot = document.createElement("span");
        dot.textContent = skill.active ? "●" : "○";

        const iconBox = document.createElement("span");
        iconBox.className = "skill-icon";
        if (skill.iconImage) {
            const img = document.createElement("img");
            img.src = skill.iconImage;
            img.alt = `${skill.name} icon`;
            iconBox.appendChild(img);
        } else {
            iconBox.textContent = skill.icon;
        }

        const meta = document.createElement("div");
        meta.className = "skill-meta";
        const name = document.createElement("div");
        name.className = "skill-name";
        name.textContent = skill.name;
        const level = document.createElement("div");
        level.className = "subtle";
        level.textContent = `L${skill.level} • ${skill.xp} XP`;
        const progress = document.createElement("div");
        progress.className = "progress glow";
        const bar = document.createElement("div");
        bar.className = "bar";
        progress.appendChild(bar);
        meta.appendChild(name);
        meta.appendChild(level);
        meta.appendChild(progress);

        li.appendChild(dot);
        li.appendChild(iconBox);
        li.appendChild(meta);
        li.addEventListener("click", () => selectSkill(skill.id));
        ui.skillsList.appendChild(li);
        skillUi.set(skill.id, { bar, level, dot, row: li, iconBox });
        const initialProgress = getProgress(skill);
        bar.style.width = `${initialProgress * 100}%`;
        skillProgress.set(skill.id, initialProgress);
    });
}

function ensureSkillUi() {
    if (skillUi.size !== state.skills.length) {
        renderSkills();
    }
}

function updateSkillMeta() {
    ensureSkillUi();
    state.skills.forEach((skill) => {
        const parts = skillUi.get(skill.id);
        if (!parts) return;
        parts.dot.textContent = skill.active ? "●" : "○";
        parts.level.textContent = `L${skill.level} • ${skill.xp} XP`;
    });
}

function updateSkillBars(delta) {
    ensureSkillUi();
    uiUpdates = 0;
    const now = performance.now();
    if (iconComboLevel > 0 && now > iconComboExpireAt) {
        iconComboLevel = 0;
        const icon = document.querySelector(".skill-context .skill-icon");
        if (icon) {
            icon.classList.add("combo-deflate");
            icon.style.setProperty("--icon-combo-scale", "1");
            icon.style.removeProperty("--icon-pop-scale");
            icon.style.removeProperty("--icon-pop-overshoot");
            icon.style.removeProperty("--icon-wobble-rot");
            icon.style.removeProperty("--icon-wobble-shift");
            icon.style.removeProperty("--icon-wobble-scale");
            icon.style.removeProperty("--icon-burst-duration");
            window.setTimeout(() => {
                icon.classList.remove("combo-deflate");
                icon.classList.remove("level-up-burst");
            }, 900);
        }
    }
    if (pendingIconBurstAt && now >= pendingIconBurstAt) {
        if (pendingIconBurstSkillId === selectedSkillId) {
            triggerSkillIconLevelUp();
        }
        pendingIconBurstAt = 0;
        pendingIconBurstSkillId = null;
    }
    state.skills.forEach((skill) => {
        const parts = skillUi.get(skill.id);
        if (!parts) return;
        const bar = parts.bar;
        const holdUntil = skillCompletionHold.get(skill.id) ?? 0;
        const target = holdUntil > now ? 1 : getProgress(skill);
        const current = skillProgress.get(skill.id) ?? 0;
        const smoothing = 1 - Math.exp(-delta / 0.18);
        const next = current + (target - current) * smoothing;
        const clamped = clamp(next, 0, 1);
        if (Math.abs(clamped - current) > 0.0005 && bar) {
            bar.style.width = `${clamped * 100}%`;
            uiUpdates += 1;
        }
        skillProgress.set(skill.id, clamped);
    });
    const selectedHold = skillCompletionHold.get(selectedSkillId) ?? 0;
    const selectedProgress = selectedHold > performance.now()
        ? 1
        : (skillProgress.get(selectedSkillId) ?? 0);
    const clampedProgress = clamp(selectedProgress, 0, 1);
    setRingProgress(clampedProgress);
    const selectedSkill = state.skills.find((s) => s.id === selectedSkillId);
    const isSelectedActive = Boolean(selectedSkill?.active);
    updateRingTip(clampedProgress, isSelectedActive);
    if (isSelectedActive) {
        emitRingParticles(selectedProgress, now);
    }
    ui.debugUiUpdates.textContent = `UI updates/frame: ${uiUpdates}`;
}

function emitRingParticles(progress, now) {
    if (!ui.contextRingParticles || !ui.contextRing || !ui.contextRingWrap) return;
    const intensity = clamp(progress, 0, 1);
    const interval = 220 - intensity * 170;
    if (now - lastParticleTime < interval) return;
    lastParticleTime = now;

    const count = Math.max(1, Math.round(1 + intensity * 4));
    for (let i = 0; i < count; i++) {
        spawnRingParticle(progress);
    }

    if (progress >= 0.999 && now - lastCompletionBurst > 600) {
        lastCompletionBurst = now;
        for (let i = 0; i < 14; i++) {
            spawnRingParticle(1, true);
        }
    }
}

function setRingProgress(progress) {
    if (!ui.contextRingProgress) return;
    ui.contextRingProgress.style.strokeDasharray = `${RING_CIRCUMFERENCE}`;
    ui.contextRingProgress.style.strokeDashoffset = `${RING_CIRCUMFERENCE * (1 - progress)}`;
}

function getRingMetrics() {
    if (!ui.contextRingWrap || !ui.contextRingProgress || !ui.contextRingSvg) return null;
    const wrapRect = ui.contextRingWrap.getBoundingClientRect();
    const svgRect = ui.contextRingSvg.getBoundingClientRect();
    if (!svgRect.width || !svgRect.height) return null;
    const viewBox = (ui.contextRingSvg.getAttribute("viewBox") ?? "0 0 100 100").split(" ");
    const viewSize = Number(viewBox[2]) || 100;
    const radius = Number(ui.contextRingProgress.getAttribute("r") ?? RING_RADIUS);
    const strokeWidth = Number.parseFloat(getComputedStyle(ui.contextRingProgress).strokeWidth || "0");
    const radiusPx = (svgRect.width * radius) / viewSize + strokeWidth / 2;
    return {
        centerX: svgRect.left - wrapRect.left + svgRect.width / 2,
        centerY: svgRect.top - wrapRect.top + svgRect.height / 2,
        radiusPx
    };
}

function updateRingTip(progress, isActive) {
    if (!ui.contextRing || !ui.contextRingTip || !ui.contextRingWrap) return;
    const metrics = getRingMetrics();
    if (!metrics) return;
    const tipRect = ui.contextRingTip.getBoundingClientRect();
    const radius = metrics.radiusPx;
    const angle = (progress * Math.PI * 2) - Math.PI / 2;
    const centerX = metrics.centerX;
    const centerY = metrics.centerY;
    const tipX = centerX + Math.cos(angle) * radius - tipRect.width / 2;
    const tipY = centerY + Math.sin(angle) * radius - tipRect.height / 2;
    ui.contextRingTip.style.left = `${tipX}px`;
    ui.contextRingTip.style.top = `${tipY}px`;
    ui.contextRingTip.style.opacity = isActive ? `${0.4 + clamp(progress, 0, 1) * 0.6}` : "0";
}

function spawnRingParticle(progress, burst = false) {
    if (!ui.contextRingParticles || !ui.contextRing || !ui.contextRingWrap) return;
    const particle = document.createElement("span");
    particle.className = "ring-particle";
    const metrics = getRingMetrics();
    if (!metrics) return;
    const radius = metrics.radiusPx;
    const angle = (progress * Math.PI * 2) - Math.PI / 2;
    const centerX = metrics.centerX;
    const centerY = metrics.centerY;
    const tipX = centerX + Math.cos(angle) * radius;
    const tipY = centerY + Math.sin(angle) * radius;
    const spread = burst ? 18 : 8;
    const driftX = (Math.random() - 0.5) * spread;
    const driftY = (Math.random() - 0.5) * spread - 12;
    const size = burst ? 8 : 4 + Math.random() * 4;
    const duration = burst ? 0.8 : 0.6 + Math.random() * 0.6;
    const hue = 280 + Math.random() * 60;

    particle.style.left = `${tipX}px`;
    particle.style.top = `${tipY}px`;
    particle.style.width = `${size}px`;
    particle.style.height = `${size}px`;
    particle.style.animationDuration = `${duration}s`;
    particle.style.setProperty("--dx", `${driftX}px`);
    particle.style.setProperty("--dy", `${driftY}px`);
    particle.style.background = `radial-gradient(circle, hsla(${hue}, 90%, 80%, 1), hsla(${hue}, 90%, 70%, 0.6))`;
    ui.contextRingParticles.appendChild(particle);
    particle.addEventListener("animationend", () => particle.remove());
}

function triggerContextShake() {
    if (!contextCard) return;
    contextCard.classList.remove("shake");
    void contextCard.offsetWidth;
    contextCard.classList.add("shake");
}

function pushIconCombo(levels = 1) {
    const now = performance.now();
    iconComboLevel = Math.min(8, iconComboLevel + levels);
    const extension = 1100 + iconComboLevel * 160;
    iconComboExpireAt = Math.max(iconComboExpireAt, now + extension);
    const icon = document.querySelector(".skill-context .skill-icon");
    if (icon) {
        const comboScale = 1 + iconComboLevel * 0.06;
        icon.style.setProperty("--icon-combo-scale", `${comboScale.toFixed(2)}`);
        icon.classList.remove("combo-deflate");
    }
}

function triggerSkillIconLevelUp() {
    const icon = document.querySelector(".skill-context .skill-icon");
    if (!icon) return;
    const combo = Math.max(1, iconComboLevel || 1);
    const popScale = 1.2 + combo * 0.05;
    const popOvershoot = 0.94 - combo * 0.01;
    const wobbleRot = 10 + combo * 2.2;
    const wobbleShift = 4 + combo * 0.7;
    const wobbleScale = 1.08 + combo * 0.02;
    const burstDuration = 0.5 + combo * 0.05;
    const comboScale = 1 + combo * 0.06;
    icon.style.setProperty("--icon-combo-scale", `${comboScale.toFixed(2)}`);
    icon.style.setProperty("--icon-pop-scale", `${popScale.toFixed(2)}`);
    icon.style.setProperty("--icon-pop-overshoot", `${popOvershoot.toFixed(2)}`);
    icon.style.setProperty("--icon-wobble-rot", `${wobbleRot.toFixed(1)}deg`);
    icon.style.setProperty("--icon-wobble-shift", `${wobbleShift.toFixed(1)}px`);
    icon.style.setProperty("--icon-wobble-scale", `${wobbleScale.toFixed(2)}`);
    icon.style.setProperty("--icon-burst-duration", `${burstDuration.toFixed(2)}s`);
    icon.classList.remove("level-up-burst");
    void icon.offsetWidth;
    icon.classList.add("level-up-burst");
    if (iconBurstTimeoutId) {
        clearTimeout(iconBurstTimeoutId);
    }
    iconBurstTimeoutId = window.setTimeout(() => {
        if (performance.now() >= iconComboExpireAt) {
            icon.classList.remove("level-up-burst");
        }
    }, Math.round(burstDuration * 1000));

    let burst = icon.querySelector(".skill-icon-burst");
    if (!burst) {
        burst = document.createElement("span");
        burst.className = "skill-icon-burst";
        burst.setAttribute("aria-hidden", "true");
        icon.appendChild(burst);
    }

    burst.innerHTML = "";
    const particleCount = 18 + Math.round(combo * 2);
    for (let i = 0; i < particleCount; i += 1) {
        const particle = document.createElement("span");
        particle.className = "skill-icon-particle";
        const angle = (Math.PI * 2 * i) / particleCount + Math.random() * 0.35;
        const distance = (28 + Math.random() * 36) * (1 + combo * 0.08);
        const dx = Math.cos(angle) * distance;
        const dy = Math.sin(angle) * distance;
        const size = 4 + Math.random() * 4 + combo * 0.4;
        particle.style.setProperty("--dx", `${dx}px`);
        particle.style.setProperty("--dy", `${dy}px`);
        particle.style.setProperty("--size", `${size}px`);
        burst.appendChild(particle);
        particle.addEventListener("animationend", () => particle.remove(), { once: true });
    }
}

function triggerSkillListLevelUp(skillId) {
    const entry = skillUi.get(skillId);
    if (!entry?.row) return;
    entry.row.classList.remove("level-up");
    void entry.row.offsetWidth;
    entry.row.classList.add("level-up");
    entry.row.addEventListener("animationend", () => entry.row.classList.remove("level-up"), { once: true });
}

function renderSkillContext() {
    const skill = state.skills.find((s) => s.id === selectedSkillId) ?? state.skills[0];
    selectedSkillId = skill.id;
    const iconContainer = document.querySelector(".skill-context .skill-icon");
    if (iconContainer) {
        iconContainer.classList.toggle("is-active", Boolean(skill.active));
    }
    if (skill.iconImage) {
        ui.skillIconImg.src = skill.iconImage;
        ui.skillIconImg.style.display = "block";
        ui.skillIcon.style.display = "none";
    } else {
        ui.skillIconImg.style.display = "none";
        ui.skillIcon.style.display = "inline";
        ui.skillIcon.textContent = skill.icon;
    }
    ui.skillName.textContent = skill.name.toUpperCase();
    if (ui.skillLevel) {
        ui.skillLevel.textContent = `${skill.level}`;
    }
    const nextXp = xpForLevel(skill.level + 1);
    ui.skillDetails.textContent = `${skill.xp.toLocaleString()} / ${nextXp.toLocaleString()} XP`;
    if (ui.contextRing) {
        const holdUntil = skillCompletionHold.get(skill.id) ?? 0;
        const progress = holdUntil > performance.now() ? 1 : (skillProgress.get(skill.id) ?? getProgress(skill));
        const clampedProgress = clamp(progress, 0, 1);
        setRingProgress(clampedProgress);
        updateRingTip(clampedProgress, Boolean(skill.active));
    }

    ui.taskList.innerHTML = "";
    skill.tasks.forEach((task) => {
        const li = document.createElement("li");
        const locked = skill.level < task.level;
        const rewardItem = task.rewardItemId ? ITEM_DEFS[task.rewardItemId] : null;
        const rewardIcon = rewardItem
            ? `<img src="${rewardItem.icon}" alt="${rewardItem.name}" class="item-icon" />`
            : "";
        const taskIcon = task.iconImage
            ? `<img src="${task.iconImage}" alt="${task.label}" class="item-icon task-icon" />`
            : `<span>${locked ? "🔒" : "✅"}</span>`;
        const labelSuffix = locked ? " • Locked" : "";
        li.innerHTML = `${taskIcon}<div><div>${task.label}${labelSuffix}</div><div class="subtle">Level ${task.level} • ${task.reward}</div></div><span class="reward-icon">${rewardIcon}</span>`;
        li.style.opacity = locked ? "0.5" : "1";
        if (!locked) {
            li.addEventListener("click", () => {
                skill.task = task.id;
                pendingTaskBounceId = task.id;
                renderSkillContext();
            });
        }
        if (skill.task === task.id) {
            li.classList.add("selected");
        }
        if (pendingTaskBounceId === task.id) {
            const iconEl = li.querySelector(".task-icon");
            if (iconEl) {
                iconEl.classList.add("task-bounce");
                iconEl.addEventListener("animationend", () => iconEl.classList.remove("task-bounce"), {
                    once: true
                });
            }
        }
        ui.taskList.appendChild(li);
    });
    pendingTaskBounceId = null;

    ui.taskAction.textContent = skill.active
        ? "Stop"
        : skill.id === "gambling"
            ? "Play"
            : "Idle";
    ui.taskActionHint.textContent = skill.active
        ? `Training ${skill.task} in the background.`
        : `Start ${skill.task} to gain XP and rewards.`;
}

function selectSkill(skillId) {
    selectedSkillId = skillId;
    renderSkillContext();
}

function toggleSkillAction() {
    const skill = state.skills.find((s) => s.id === selectedSkillId);
    if (!skill) return;
    const currentlyActive = state.skills.find((s) => s.active);
    if (currentlyActive && currentlyActive.id !== skill.id) {
        currentlyActive.active = false;
    }
    skill.active = !skill.active;
    updateSkillMeta();
    renderSkillContext();
}

function boostSkillLevels(skillId, amount) {
    const skill = state.skills.find((s) => s.id === skillId);
    if (!skill || amount <= 0) return;
    const previousLevel = skill.level;
    const nextLevel = clamp(skill.level + amount, 1, skill.maxLevel);
    skill.level = nextLevel;
    skill.xp = xpForLevel(nextLevel);
    const gained = nextLevel - previousLevel;
    if (gained > 0) {
        const holdUntil = performance.now() + 450;
        skillCompletionHold.set(skill.id, holdUntil);
        if (skill.id === selectedSkillId) {
            triggerContextShake();
            pushIconCombo(gained);
            pendingIconBurstAt = holdUntil - 220;
            pendingIconBurstSkillId = skill.id;
        } else {
            triggerSkillListLevelUp(skill.id);
        }
    }
    updateSkillMeta();
    renderSkillContext();
    updateLists();
    updateInspectors();
}

function applySkillXp(skill, amount, triggerEffects) {
    if (!skill || amount <= 0) return;
    const previousLevel = skill.level;
    skill.xp += amount;
    while (skill.level < skill.maxLevel && skill.xp >= xpForLevel(skill.level + 1)) {
        skill.level += 1;
    }
    const gained = skill.level - previousLevel;
    if (gained > 0 && triggerEffects) {
        const holdUntil = performance.now() + 450;
        skillCompletionHold.set(skill.id, holdUntil);
        if (skill.id === selectedSkillId) {
            triggerContextShake();
            pushIconCombo(gained);
            pendingIconBurstAt = holdUntil - 220;
            pendingIconBurstSkillId = skill.id;
        } else {
            triggerSkillListLevelUp(skill.id);
        }
    }
}

function tickOnce() {
    if (state.isLocked) return;
    state.tick += 1;
    const active = state.skills.find((s) => s.active);
    if (active) {
        const xpGain = active.id === "gambling" ? 18 : 12;
        applySkillXp(active, xpGain, true);
        if (active.id === "gambling") {
            if (active.task === "scratchers") {
                const reward = randInt(1, 3);
                addItemToInventory("chip_green", reward);
            }
            if (active.task === "keno") {
                const reward = randInt(1, 3);
                addItemToInventory("chip_blue", reward);
            }
            state.wallet.cash += 2;
        } else {
            state.wallet.cash += 1;
        }
    }
    const combatLine = state.tick % 2 === 0 ? "player hits slime (3)" : "slime misses";
    state.combat.unshift(combatLine);
    state.combat = state.combat.slice(0, 5);
    ui.debugTickRate.textContent = `Tick rate: ${state.tickRate}/s • Tick ${state.tick}`;
    updateLists();
}

function runOffline(seconds) {
    lastOfflineSeconds = seconds;
    ui.debugOnline.textContent = "Offline (reconciling)";
    ui.debugOffline.textContent = `Last offline: ${seconds}s`;
    for (let i = 0; i < seconds; i++) {
        tickOnce();
    }
    ui.debugOnline.textContent = "Online";
}

function saveSnapshot() {
    const snapshot = {
        tick: state.tick,
        wallet: state.wallet,
        inventorySlots: state.inventorySlots,
        skills: state.skills.map((skill) => ({
            id: skill.id,
            level: skill.level,
            xp: skill.xp,
            active: skill.active,
            task: skill.task
        })),
        saveStatus: "Saved"
    };
    localStorage.setItem(STORAGE_KEY, JSON.stringify(snapshot));
    state.saveStatus = "Saved";
    updateLists();
}

function loadSnapshot() {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) {
        state.saveStatus = "No snapshot";
        updateLists();
        return;
    }

    const snapshot = JSON.parse(raw);
    state.tick = snapshot.tick ?? 0;
    state.wallet = snapshot.wallet ?? state.wallet;
    if (Array.isArray(snapshot.inventorySlots)) {
        state.inventorySlots = snapshot.inventorySlots;
    } else if (snapshot.inventory && typeof snapshot.inventory === "object") {
        state.inventorySlots = Object.entries(snapshot.inventory)
            .filter(([, amount]) => amount > 0)
            .flatMap(([id, amount]) => {
                const def = getItemDef(id);
                const limit = getStackLimit(def);
                const slots = [];
                let remaining = amount;
                while (remaining > 0) {
                    const toAdd = def.stackable === false || def.unique ? 1 : Math.min(limit, remaining);
                    slots.push({ id, quantity: toAdd });
                    remaining -= toAdd;
                }
                return slots;
            });
    }
    state.saveStatus = "Loaded";
    if (Array.isArray(snapshot.skills)) {
        snapshot.skills.forEach((entry) => {
            const skill = state.skills.find((s) => s.id === entry.id);
            if (!skill) return;
            skill.level = entry.level ?? skill.level;
            skill.xp = entry.xp ?? skill.xp;
            skill.active = entry.active ?? false;
            skill.task = entry.task ?? entry.mode ?? skill.task;
        });
    }

    updateLists();
}

function updateLists() {
    const totals = getInventoryTotals();
    const green = totals.chip_green ?? 0;
    const blue = totals.chip_blue ?? 0;
    state.quests = [
        `Win Scratchers (${Math.min(green, 5)}/5)`,
        `Play Keno (${Math.min(blue, 3)}/3)`
    ];
    state.achievements = [
        `High Roller (${Math.min(state.wallet.cash, 100)}/100)`,
        `Stack Chips (${Math.min(green + blue, 12)}/12)`
    ];
    const collectionCount = [green > 0, blue > 0, true].filter(Boolean).length;
    state.collections = [`Gambling Set: ${collectionCount}/3`];

    renderWallet();
    renderInventory();
    updateSkillMeta();
    renderSkillContext();
    renderList(ui.worldList, state.world);
    renderList(ui.equipmentList, state.equipment);
    renderList(ui.questList, state.quests);
    renderList(ui.achievementList, state.achievements);
    renderList(ui.collectionList, state.collections);
    renderList(ui.combatList, state.combat);
    renderList(ui.craftingList, state.crafting);
    renderList(ui.tradeList, state.trade);
    renderList(ui.generatorList, state.generator);
    renderList(ui.compendiumList, state.compendium);
    ui.saveStatus.textContent = state.saveStatus;
    updateInspectors();
}

function updateInspectors() {
    ui.walletInspector.innerHTML = "";
    Object.entries(state.wallet).forEach(([name, value]) => {
        const currency = CURRENCY_DEFS[name] ?? { name, icon: null };
        const row = document.createElement("div");
        row.className = "inspector-row";
        const iconMarkup = currency.icon
            ? `<img src="${currency.icon}" alt="${currency.name}" class="item-icon" />`
            : `<span class="item-icon">${name.slice(0, 1).toUpperCase()}</span>`;
        row.innerHTML = `${iconMarkup}<span>${currency.name}</span><span class="subtle">${value}</span>`;
        const add = document.createElement("button");
        add.className = "button";
        add.textContent = "+10";
        add.onclick = () => {
            state.wallet[name] += 10;
            updateLists();
            updateInspectors();
        };
        const remove = document.createElement("button");
        remove.className = "button";
        remove.textContent = "-10";
        remove.onclick = () => {
            state.wallet[name] = Math.max(0, state.wallet[name] - 10);
            updateLists();
            updateInspectors();
        };
        row.appendChild(add);
        row.appendChild(remove);
        ui.walletInspector.appendChild(row);
    });

    ui.inventoryInspector.innerHTML = "";
    const totals = getInventoryTotals();
    Object.entries(totals).forEach(([name, value]) => {
        const item = getItemDef(name);
        const row = document.createElement("div");
        row.className = "inspector-row";
        const iconMarkup = item.icon
            ? `<img src="${item.icon}" alt="${item.name}" class="item-icon" />`
            : `<span class="item-icon">${name.slice(0, 1).toUpperCase()}</span>`;
        row.innerHTML = `${iconMarkup}<span>${item.name}</span><span class="subtle">${value}</span>`;
        const add = document.createElement("button");
        add.className = "button";
        add.textContent = "+1";
        add.onclick = () => {
            addItemToInventory(name, 1);
            updateLists();
            updateInspectors();
        };
        const remove = document.createElement("button");
        remove.className = "button";
        remove.textContent = "-1";
        remove.onclick = () => {
            removeItemFromInventory(name, 1);
            updateLists();
            updateInspectors();
        };
        row.appendChild(add);
        row.appendChild(remove);
        ui.inventoryInspector.appendChild(row);
    });

    ui.skillInspector.innerHTML = "";
    state.skills.forEach((skill) => {
        const row = document.createElement("div");
        row.className = "inspector-row";
        row.innerHTML = `<span>${skill.name}</span><span class="subtle">L${skill.level} • ${skill.xp} XP</span>`;
        const addSmall = document.createElement("button");
        addSmall.className = "button";
        addSmall.textContent = "+100 XP";
        addSmall.onclick = () => {
            applySkillXp(skill, 100, true);
            updateLists();
            updateInspectors();
        };
        const addLarge = document.createElement("button");
        addLarge.className = "button";
        addLarge.textContent = "+1000 XP";
        addLarge.onclick = () => {
            applySkillXp(skill, 1000, true);
            updateLists();
            updateInspectors();
        };
        row.appendChild(addSmall);
        row.appendChild(addLarge);
        ui.skillInspector.appendChild(row);
    });
}

function animate(now) {
    if (!running) return;
    const delta = (now - lastTick) / 1000;
    if (!Number.isFinite(delta) || delta <= 0) {
        requestAnimationFrame(animate);
        return;
    }
    lastTick = now;
    fpsFrames += 1;
    fpsElapsed += delta;
    if (fpsElapsed >= 1) {
        const fps = Math.round(fpsFrames / fpsElapsed);
        ui.debugFps.textContent = `FPS: ${fps}`;
        fpsFrames = 0;
        fpsElapsed = 0;
    }
    updateSkillBars(delta);
    lastUiUpdateCount = uiUpdates;
    requestAnimationFrame(animate);
}

function setupTabs() {
    document.querySelectorAll(".tabs .tab-button").forEach((button) => {
        button.addEventListener("click", () => {
            const target = button.dataset.tab;
            const container = button.closest(".tabs");
            container.querySelectorAll(".tab-button").forEach((btn) => btn.classList.remove("active"));
            container.querySelectorAll(".tab-panel").forEach((panel) => panel.classList.remove("active"));
            button.classList.add("active");
            container.querySelector(`.tab-panel[data-panel='${target}']`).classList.add("active");
        });
    });

    document.querySelectorAll(".overlay .tab-button").forEach((button) => {
        button.addEventListener("click", () => {
            const target = button.dataset.tab;
            const container = button.closest(".overlay-panel");
            container.querySelectorAll(".tab-button").forEach((btn) => btn.classList.remove("active"));
            container.querySelectorAll(".tab-panel").forEach((panel) => panel.classList.remove("active"));
            button.classList.add("active");
            container.querySelector(`.tab-panel[data-panel='${target}']`).classList.add("active");
        });
    });
}

function setupSplitters() {
    const layout = document.querySelector(".layout");
    const right = document.getElementById("rightPanel");
    const splitter = document.getElementById("splitterVertical");
    const splitterHorizontal = document.getElementById("splitterHorizontal");
    const gameView = document.getElementById("gameView");
    let draggingVertical = false;
    let draggingHorizontal = false;

    splitter.addEventListener("pointerdown", (event) => {
        draggingVertical = true;
        splitter.setPointerCapture(event.pointerId);
    });

    splitterHorizontal.addEventListener("pointerdown", (event) => {
        draggingHorizontal = true;
        splitterHorizontal.setPointerCapture(event.pointerId);
    });

    window.addEventListener("pointermove", (event) => {
        if (draggingVertical) {
            const rect = layout.getBoundingClientRect();
            const width = clamp(event.clientX - rect.left, 220, 520);
            layout.style.gridTemplateColumns = `${width}px 8px minmax(420px, 1fr)`;
        }

        if (draggingHorizontal) {
            const rect = right.getBoundingClientRect();
            const height = clamp(event.clientY - rect.top, 180, 420);
            gameView.style.height = `${height}px`;
        }
    });

    window.addEventListener("pointerup", () => {
        draggingVertical = false;
        draggingHorizontal = false;
    });
}

function setupControls() {
    document.getElementById("openDebug").onclick = () => ui.debugOverlay.classList.remove("hidden");
    document.getElementById("closeDebug").onclick = () => ui.debugOverlay.classList.add("hidden");
    ui.taskAction.onclick = toggleSkillAction;
    document.getElementById("tickBtn").onclick = tickOnce;
    document.getElementById("offlineBtn").onclick = () => runOffline(10);
    document.getElementById("addGoldBtn").onclick = () => {
        state.wallet.gold += 10;
        updateLists();
        updateInspectors();
    };
    document.getElementById("saveBtn").onclick = saveSnapshot;
    document.getElementById("loadBtn").onclick = loadSnapshot;
    document.getElementById("toggleSandbox").onclick = () => {
        state.sandboxEnabled = !state.sandboxEnabled;
        refreshToggleButtons();
    };
    document.getElementById("toggleLock").onclick = () => {
        state.isLocked = !state.isLocked;
        refreshToggleButtons();
    };

    const levelButtons = document.querySelectorAll("[data-level-boost]");
    levelButtons.forEach((btn) => {
        btn.addEventListener("click", () => {
            const boost = Number(btn.getAttribute("data-level-boost") || 0);
            boostSkillLevels(selectedSkillId, boost);
        });
    });
    const levelInput = document.getElementById("levelBoostInput");
    const levelApply = document.getElementById("levelBoostApply");
    if (levelInput && levelApply) {
        levelApply.addEventListener("click", () => {
            const boost = Math.floor(Number(levelInput.value || 0));
            if (boost > 0) {
                boostSkillLevels(selectedSkillId, boost);
                levelInput.value = "";
            }
        });
    }

    ui.zoomSlider.oninput = (event) => {
        state.zoom = Number(event.target.value);
        document.getElementById("app").style.transform = `scale(${state.zoom})`;
        document.getElementById("app").style.transformOrigin = "top left";
        ui.zoomValue.textContent = `${Math.round(state.zoom * 100)}%`;
    };
}

function refreshToggleButtons() {
    const sandboxBtn = document.getElementById("toggleSandbox");
    const lockBtn = document.getElementById("toggleLock");
    sandboxBtn.textContent = state.sandboxEnabled ? "🧪 Sandbox On" : "🧪 Sandbox Off";
    lockBtn.textContent = state.isLocked ? "🔒 Locked" : "🔓 Unlocked";
}

async function setupPixi() {
    if (!window.PIXI) {
        console.warn("PIXI not loaded - skipping renderer init.");
        return;
    }

    const canvas = document.getElementById("pixiCanvas");
    if (!canvas) {
        console.warn("PIXI canvas not found - skipping renderer init.");
        return;
    }

    try {
        const app = new PIXI.Application();
        await app.init({
            backgroundColor: 0x0c111b,
            resizeTo: canvas.parentElement ?? window,
            antialias: true,
            canvas
        });

        const bg = new PIXI.Graphics();
        bg.beginFill(0x111827);
        bg.drawRoundedRect(0, 0, 600, 280, 16);
        bg.endFill();
        app.stage.addChild(bg);

        const player = new PIXI.Graphics();
        player.beginFill(0x60a5fa);
        player.drawCircle(0, 0, 14);
        player.endFill();
        player.x = 80;
        player.y = 120;
        app.stage.addChild(player);

        const tree = new PIXI.Graphics();
        tree.beginFill(0x22c55e);
        tree.drawRoundedRect(-10, -40, 20, 40, 6);
        tree.endFill();
        tree.x = 250;
        tree.y = 140;
        app.stage.addChild(tree);

        app.ticker.add(() => {
            player.x = 80 + Math.sin(performance.now() / 600) * 20;
            player.y = 120 + Math.cos(performance.now() / 700) * 10;
        });
    } catch (error) {
        console.error("PIXI init failed", error);
    }
}

function setupPwa() {
    if ("serviceWorker" in navigator) {
        if (location.hostname === "localhost" || location.hostname === "127.0.0.1") {
            navigator.serviceWorker.getRegistrations().then((regs) => regs.forEach((reg) => reg.unregister()));
            return;
        }
        navigator.serviceWorker.register("service-worker.js");
    }
}

async function main() {
    ui.debugTickRate.textContent = `Tick rate: ${state.tickRate}/s`;
    ui.debugOnline.textContent = "Online";
    ui.debugOffline.textContent = `Last offline: ${lastOfflineSeconds}s`;
    ui.debugUiUpdates.textContent = "UI updates/frame: --";
    renderSkills();
    updateLists();
    updateInspectors();
    refreshToggleButtons();
    renderSkillContext();
    setupTabs();
    setupSplitters();
    setupControls();
    await setupPixi();
    setupPwa();
    document.addEventListener("click", () => {
        if (popoverPinned) {
            popoverPinned = false;
            hideInventoryPopover();
        }
    });
    lastTick = performance.now();
    requestAnimationFrame(animate);
    setInterval(() => {
        if (!state.isLocked) {
            tickOnce();
        }
    }, 1000 / state.tickRate);
}

main().catch((error) => {
    console.error("Web demo failed to start", error);
});

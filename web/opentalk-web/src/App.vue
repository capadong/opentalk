<template>
  <router-view />
</template>

<style>
@import url('https://fonts.googleapis.com/css2?family=Noto+Sans+SC:wght@400;500;600;700;800&display=swap');

/* ===================== OpenTalk Design Tokens ===================== */
:root {
  /* Base backgrounds */
  --ot-bg-base:        #0d1117;
  --ot-bg-sidebar:     #0a0e14;
  --ot-bg-panel:       #111720;
  --ot-bg-input:       #161d28;
  --ot-bg-hover:       rgba(99,179,237,0.07);
  --ot-bg-active:      rgba(99,179,237,0.12);
  --ot-bg-bubble-other: #1a2333;

  /* Borders */
  --ot-border:         rgba(255,255,255,0.07);

  /* Text */
  --ot-text-primary:   #e2e8f0;
  --ot-text-secondary: #94a3b8;
  --ot-text-muted:     #4a5568;

  /* Accent — electric cyan-blue */
  --ot-accent:         #63b3ed;
  --ot-accent-dim:     rgba(99,179,237,0.15);
  --ot-accent-glow:    rgba(99,179,237,0.18);

  /* Self message bubble gradient */
  --ot-accent-self-a:  #3b82f6;
  --ot-accent-self-b:  #6366f1;

  /* Status */
  --ot-green:          #34d399;
  --ot-red:            #fc4b6c;
  --ot-red-dim:        rgba(252,75,108,0.25);
  --ot-red-glow:       rgba(252,75,108,0.08);

  /* Avatar gradient palette defaults */
  --ot-avatar-a:       #3b82f6;
  --ot-avatar-b:       #6366f1;
}

/* ===================== Global Reset ===================== */
*, *::before, *::after {
  box-sizing: border-box;
}

html, body {
  height: 100%;
  margin: 0;
  padding: 0;
  background: var(--ot-bg-base);
  color: var(--ot-text-primary);
  font-family: 'Noto Sans SC', 'PingFang SC', 'Microsoft YaHei', sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}

#app {
  height: 100%;
  display: flex;
  flex-direction: column;
}

/* ===================== Element Plus dark overrides ===================== */
.el-dialog {
  background: var(--ot-bg-panel) !important;
  border: 1px solid var(--ot-border) !important;
  border-radius: 14px !important;
  box-shadow: 0 24px 80px rgba(0,0,0,0.6) !important;
}

.el-dialog__header {
  border-bottom: 1px solid var(--ot-border) !important;
  padding: 16px 20px !important;
}

.el-dialog__title {
  color: var(--ot-text-primary) !important;
  font-size: 15px !important;
  font-weight: 600 !important;
}

.el-dialog__body {
  color: var(--ot-text-primary) !important;
  padding: 16px 20px !important;
}

.el-dialog__footer {
  border-top: 1px solid var(--ot-border) !important;
  padding: 12px 20px !important;
}

.el-notification {
  background: var(--ot-bg-panel) !important;
  border: 1px solid var(--ot-border) !important;
  border-radius: 10px !important;
  box-shadow: 0 8px 32px rgba(0,0,0,0.5) !important;
}

.el-notification__title {
  color: var(--ot-text-primary) !important;
}

.el-notification__content {
  color: var(--ot-text-secondary) !important;
}

.el-message {
  background: var(--ot-bg-panel) !important;
  border: 1px solid var(--ot-border) !important;
  border-radius: 8px !important;
}

/* Scrollbar global */
::-webkit-scrollbar { width: 5px; height: 5px; }
::-webkit-scrollbar-track { background: transparent; }
::-webkit-scrollbar-thumb { background: rgba(255,255,255,0.1); border-radius: 3px; }
::-webkit-scrollbar-thumb:hover { background: rgba(255,255,255,0.18); }
</style>

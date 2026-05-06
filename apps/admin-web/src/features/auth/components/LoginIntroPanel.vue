<template>
  <article class="login-intro-panel">
    <div class="login-intro-panel__eyebrow">Explore Admin</div>

    <div class="login-intro-panel__content">
      <h1>管理秩序，自此进入</h1>
      <p>为内部管理与运营协同提供统一、稳定且可审计的工作入口。</p>
    </div>

    <div class="login-intro-panel__notes">
      <div class="login-intro-panel__note">
        <span class="login-intro-panel__note-index">01</span>
        <div class="login-intro-panel__note-copy">
          <strong>统一身份边界</strong>
          <small>Access Control</small>
        </div>
      </div>
      <div class="login-intro-panel__note">
        <span class="login-intro-panel__note-index">02</span>
        <div class="login-intro-panel__note-copy">
          <strong>稳定权限秩序</strong>
          <small>Role Context</small>
        </div>
      </div>
      <div class="login-intro-panel__note">
        <span class="login-intro-panel__note-index">03</span>
        <div class="login-intro-panel__note-copy">
          <strong>关键操作更可控</strong>
          <small>Secure Session</small>
        </div>
      </div>
    </div>

    <p class="login-intro-panel__footnote">仅向已授权管理员开放</p>
  </article>
</template>

<style scoped>
.login-intro-panel {
  display: grid;
  gap: clamp(24px, 3.4vw, 44px);
  align-content: center;
  min-height: 0;
  padding: clamp(32px, 5vw, 72px) clamp(20px, 3vw, 44px) clamp(32px, 4vw, 56px);
  color: #f4f8ff;
  font-family: 'Fira Sans', 'Segoe UI', 'PingFang SC', 'Microsoft YaHei', sans-serif;
}

.login-intro-panel__eyebrow {
  display: inline-flex;
  width: fit-content;
  align-items: center;
  gap: 10px;
  color: rgba(192, 222, 255, 0.74);
  font-size: 12px;
  font-weight: 600;
  letter-spacing: 0.18em;
  text-transform: uppercase;
  font-family: 'Fira Code', 'Consolas', monospace;
}

.login-intro-panel__eyebrow::before {
  width: 28px;
  height: 1px;
  background: rgba(108, 168, 255, 0.48);
  content: '';
}

.login-intro-panel__content {
  display: grid;
  gap: 18px;
}

.login-intro-panel__content h1 {
  max-width: 620px;
  margin: 0;
  color: #f4f8ff;
  font-size: clamp(40px, 5.1vw, 64px);
  font-weight: 700;
  line-height: 1.02;
  letter-spacing: -0.04em;
}

.login-intro-panel__content p {
  max-width: 440px;
  margin: 0;
  color: rgba(220, 232, 249, 0.72);
  font-size: clamp(16px, 1.35vw, 18px);
  line-height: 1.85;
}

.login-intro-panel__notes {
  display: grid;
  gap: 16px;
  max-width: 360px;
}

.login-intro-panel__note {
  display: grid;
  grid-template-columns: 42px 1fr;
  gap: 14px;
  align-items: start;
  padding-top: 14px;
  border-top: 1px solid rgba(175, 214, 255, 0.08);
  color: #f4f8ff;
  font-size: 15px;
  line-height: 1.6;
}

.login-intro-panel__note-index {
  color: rgba(108, 168, 255, 0.72);
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.14em;
  font-family: 'Fira Code', 'Consolas', monospace;
}

.login-intro-panel__note-copy {
  display: grid;
  gap: 4px;
}

.login-intro-panel__note-copy strong {
  color: #f4f8ff;
  font-size: 15px;
  font-weight: 600;
}

.login-intro-panel__note-copy small,
.login-intro-panel__footnote {
  margin: 0;
  color: rgba(183, 204, 232, 0.56);
  font-size: 12px;
  line-height: 1.7;
  font-family: 'Fira Code', 'Consolas', monospace;
}

.login-intro-panel__footnote {
  width: fit-content;
  padding-top: 8px;
  border-top: 1px solid rgba(175, 214, 255, 0.08);
}

@media (max-width: 1024px) {
  .login-intro-panel {
    gap: 24px;
    padding: 28px 28px 8px;
  }

  .login-intro-panel__content h1 {
    max-width: 520px;
    font-size: clamp(34px, 8vw, 56px);
  }

  .login-intro-panel__content p {
    max-width: 460px;
  }

  .login-intro-panel__notes {
    max-width: 460px;
  }
}

@media (max-width: 768px) {
  .login-intro-panel {
    gap: 20px;
    padding: 20px 16px 0;
  }

  .login-intro-panel__content h1 {
    font-size: clamp(32px, 11vw, 44px);
  }

  .login-intro-panel__content p {
    max-width: 100%;
    font-size: 15px;
    line-height: 1.8;
  }

  .login-intro-panel__notes {
    gap: 12px;
    max-width: 100%;
  }

  .login-intro-panel__note {
    grid-template-columns: 34px 1fr;
    gap: 12px;
  }
}
</style>

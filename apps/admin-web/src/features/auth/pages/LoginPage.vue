<script setup lang="ts">
import LoginAuthPanel from '@/features/auth/components/LoginAuthPanel.vue'
import LoginIntroPanel from '@/features/auth/components/LoginIntroPanel.vue'
</script>

<template>
  <div class="login-page app-page">
    <section class="login-page__shell">
      <LoginIntroPanel class="login-page__intro" />
      <LoginAuthPanel class="login-page__auth" />
    </section>
  </div>
</template>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Fira+Code:wght@400;500;600&family=Fira+Sans:wght@400;500;600;700&display=swap');

.login-page {
  --login-page-padding: clamp(28px, 4vw, 44px);
  position: relative;
  min-height: 100dvh;
  overflow-x: hidden;
  overflow-y: auto;
  padding: var(--login-page-padding);
  background:
    radial-gradient(circle at top left, rgba(108, 168, 255, 0.12) 0%, transparent 22%),
    radial-gradient(circle at right 18% bottom 14%, rgba(74, 106, 255, 0.08) 0%, transparent 20%),
    linear-gradient(180deg, #0d1830 0%, #122241 42%, #152644 100%);
}

.login-page::before,
.login-page::after {
  position: absolute;
  pointer-events: none;
  content: '';
}

.login-page::before {
  inset: 0;
  background:
    linear-gradient(rgba(255, 255, 255, 0.02) 1px, transparent 1px),
    linear-gradient(90deg, rgba(255, 255, 255, 0.02) 1px, transparent 1px);
  background-size: 40px 40px;
  mask-image: linear-gradient(180deg, rgba(0, 0, 0, 0.65), transparent 92%);
}

.login-page::after {
  inset: 0;
  background:
    radial-gradient(circle at top center, rgba(108, 168, 255, 0.04), transparent 24%);
}

.login-page__shell {
  position: relative;
  z-index: 1;
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(360px, 396px);
  gap: clamp(40px, 6vw, 104px);
  min-height: calc(100dvh - (var(--login-page-padding) * 2));
  max-width: 1240px;
  margin: 0 auto;
  align-items: center;
}

.login-page__shell::before {
  position: absolute;
  inset: 0;
  z-index: -1;
  border: 1px solid rgba(148, 188, 255, 0.12);
  border-radius: 32px;
  background:
    linear-gradient(180deg, rgba(10, 20, 40, 0.78), rgba(15, 30, 55, 0.64)),
    radial-gradient(circle at top left, rgba(108, 168, 255, 0.06), transparent 24%);
  box-shadow:
    0 28px 60px rgba(4, 10, 24, 0.4),
    inset 0 1px 0 rgba(195, 223, 255, 0.08),
    inset 0 0 0 1px rgba(24, 54, 108, 0.22);
  backdrop-filter: blur(18px);
  content: '';
}

.login-page__shell::after {
  position: absolute;
  top: clamp(64px, 10vh, 88px);
  bottom: clamp(64px, 10vh, 88px);
  right: calc(396px + (clamp(40px, 6vw, 104px) / 2));
  width: 1px;
  background: linear-gradient(180deg, transparent, rgba(189, 221, 255, 0.1), transparent);
  content: '';
}

.login-page__intro,
.login-page__auth {
  min-width: 0;
  min-height: 0;
}

@media (max-width: 1024px) {
  .login-page__shell {
    grid-template-columns: 1fr;
    grid-template-rows: auto auto;
    max-width: 580px;
    gap: 24px;
    min-height: calc(100dvh - (var(--login-page-padding) * 2));
    align-items: start;
    justify-items: stretch;
  }

  .login-page__shell::after {
    display: none;
  }
}

@media (max-width: 768px) {
  .login-page {
    --login-page-padding: 16px;
  }

  .login-page::before,
  .login-page::after,
  .login-page__shell::after {
    display: none;
  }

  .login-page__intro {
    display: none;
  }

  .login-page__shell {
    grid-template-columns: 1fr;
    gap: 0;
    min-height: auto;
    max-width: 440px;
    align-items: start;
  }

  .login-page__shell::before {
    inset: 8px 0 0;
    border-radius: 28px;
    border-color: rgba(148, 188, 255, 0.08);
    background:
      linear-gradient(180deg, rgba(10, 20, 40, 0.54), rgba(15, 30, 55, 0.36)),
      radial-gradient(circle at top left, rgba(108, 168, 255, 0.04), transparent 24%);
    box-shadow:
      0 20px 44px rgba(4, 10, 24, 0.24),
      inset 0 1px 0 rgba(195, 223, 255, 0.06),
      inset 0 0 0 1px rgba(24, 54, 108, 0.14);
    backdrop-filter: blur(12px);
  }
}

@media (prefers-reduced-motion: reduce) {
  .login-page::before,
  .login-page::after {
    animation: none;
  }
}
</style>

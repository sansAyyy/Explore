<script setup lang="ts">
import type { FormInstance } from 'element-plus'

import { ElMessage } from 'element-plus'
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'

import { useLoginInteraction } from '@/features/auth/composables/useLoginInteraction'
import {
  accountLoginRules,
  buildAccountLoginPayload,
  buildPhoneLoginPayload,
  phoneLoginRules,
  PHONE_LOGIN_HELPER_TEXT,
} from '@/features/auth/models/loginForms'
import { extractErrorMessage } from '@/shared/api/error'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
const route = useRoute()
const router = useRouter()
const accountFormRef = ref<FormInstance>()
const phoneFormRef = ref<FormInstance>()

const {
  accountFormModel,
  activeMode,
  canSendPhoneCode,
  isAccountSubmitting,
  isPhoneSubmitting,
  isSendingPhoneCode,
  phoneCodeButtonText,
  phoneFormModel,
  runAccountLogin,
  runPhoneLogin,
  sendPhoneCode,
} = useLoginInteraction()

function resolveRedirectPath() {
  return typeof route.query.redirect === 'string' && route.query.redirect.startsWith('/')
    ? route.query.redirect
    : '/dashboard'
}

async function navigateAfterLogin() {
  await router.replace(resolveRedirectPath())
  ElMessage.success('登录成功')
}

async function handleAccountSubmit() {
  const form = accountFormRef.value

  if (!form) {
    return
  }

  const isValid = await form.validate().catch(() => false)

  if (!isValid) {
    return
  }

  try {
    const result = await runAccountLogin(() =>
      authStore.login(buildAccountLoginPayload(accountFormModel)),
    )

    if (result === null) {
      return
    }

    await navigateAfterLogin()
  } catch (error) {
    ElMessage.error(extractErrorMessage(error, '账号密码登录失败，请稍后重试'))
  }
}

async function handleSendPhoneCode() {
  const form = phoneFormRef.value

  if (!form) {
    return
  }

  const isValid = await form.validateField('phoneNumber').then(() => true).catch(() => false)

  if (!isValid) {
    return
  }

  try {
    const result = await sendPhoneCode()

    if (result) {
      ElMessage.success('验证码已发送，请注意查收')
    }
  } catch (error) {
    ElMessage.error(extractErrorMessage(error, '验证码发送失败，请稍后重试'))
  }
}

async function handlePhoneSubmit() {
  const form = phoneFormRef.value

  if (!form) {
    return
  }

  const isValid = await form.validate().catch(() => false)

  if (!isValid) {
    return
  }

  try {
    const result = await runPhoneLogin(() =>
      authStore.phoneLogin(buildPhoneLoginPayload(phoneFormModel)),
    )

    if (result === null) {
      return
    }

    await navigateAfterLogin()
  } catch (error) {
    ElMessage.error(extractErrorMessage(error, '短信验证码登录失败，请稍后重试'))
  }
}
</script>

<template>
  <section class="login-auth-panel">
    <el-card shadow="never" class="login-auth-panel__card">
      <template #header>
        <div class="login-auth-panel__header">
          <span class="login-auth-panel__eyebrow">Administrator Access</span>
          <strong>登录管理平台</strong>
          <p>请选择认证方式后继续。</p>
        </div>
      </template>

      <el-tabs v-model="activeMode" stretch class="login-auth-panel__tabs">
        <el-tab-pane label="账号密码登录" name="account">
          <div class="login-auth-panel__tab-content">
            <p class="login-auth-panel__mode-copy">适用于日常管理登录。</p>

            <el-form
              ref="accountFormRef"
              :model="accountFormModel"
              :rules="accountLoginRules"
              label-position="top"
              class="login-auth-panel__form"
              @keyup.enter="handleAccountSubmit"
            >
              <el-form-item label="账号" prop="account">
                <el-input
                  v-model="accountFormModel.account"
                  placeholder="请输入管理员账号"
                  autocomplete="username"
                />
              </el-form-item>

              <el-form-item label="密码" prop="password">
                <el-input
                  v-model="accountFormModel.password"
                  type="password"
                  show-password
                  placeholder="请输入登录密码"
                  autocomplete="current-password"
                />
              </el-form-item>

              <el-button
                type="primary"
                size="large"
                class="login-auth-panel__submit"
                :loading="isAccountSubmitting"
                @click="handleAccountSubmit"
              >
                登录并进入平台
              </el-button>
            </el-form>
          </div>
        </el-tab-pane>

        <el-tab-pane label="短信验证码登录" name="phone">
          <div class="login-auth-panel__tab-content">
            <p class="login-auth-panel__mode-copy">{{ PHONE_LOGIN_HELPER_TEXT }}</p>

            <el-form
              ref="phoneFormRef"
              :model="phoneFormModel"
              :rules="phoneLoginRules"
              label-position="top"
              class="login-auth-panel__form"
              @keyup.enter="handlePhoneSubmit"
            >
              <el-form-item label="手机号" prop="phoneNumber">
                <el-input
                  v-model="phoneFormModel.phoneNumber"
                  placeholder="请输入已绑定手机号"
                  autocomplete="tel"
                />
              </el-form-item>

              <el-form-item label="验证码" prop="verificationCode">
                <div class="login-auth-panel__verification-field">
                  <el-input
                    v-model="phoneFormModel.verificationCode"
                    class="login-auth-panel__verification-input"
                    placeholder="请输入验证码"
                    autocomplete="one-time-code"
                  />

                  <el-button
                    class="login-auth-panel__code-button"
                    :disabled="!canSendPhoneCode"
                    :loading="isSendingPhoneCode"
                    @click="handleSendPhoneCode"
                  >
                    {{ phoneCodeButtonText }}
                  </el-button>
                </div>
              </el-form-item>

              <el-button
                type="primary"
                size="large"
                class="login-auth-panel__submit"
                :loading="isPhoneSubmitting"
                @click="handlePhoneSubmit"
              >
                验证并进入平台
              </el-button>
            </el-form>
          </div>
        </el-tab-pane>
      </el-tabs>
    </el-card>
  </section>
</template>

<style scoped>
.login-auth-panel {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  min-height: 0;
  padding-right: clamp(14px, 1.8vw, 28px);
}

.login-auth-panel__card {
  display: flex;
  flex-direction: column;
  width: min(100%, 396px);
  height: min(568px, 100%);
  min-height: 528px;
  position: relative;
  border: 1px solid rgba(122, 176, 255, 0.12);
  border-radius: 28px;
  background:
    linear-gradient(180deg, rgba(10, 21, 42, 0.9), rgba(14, 28, 52, 0.84));
  box-shadow:
    0 24px 56px rgba(4, 10, 24, 0.36),
    0 0 0 1px rgba(26, 66, 132, 0.18) inset,
    0 0 14px rgba(76, 132, 255, 0.08);
  backdrop-filter: blur(18px);
  overflow: hidden;
}

.login-auth-panel__card::before {
  position: absolute;
  inset: 0;
  background:
    linear-gradient(180deg, rgba(148, 201, 255, 0.04), transparent 24%),
    radial-gradient(circle at top right, rgba(108, 168, 255, 0.08), transparent 24%);
  pointer-events: none;
  content: '';
}

.login-auth-panel__card :deep(.el-card__header) {
  padding-bottom: 16px;
  border-bottom-color: rgba(183, 216, 255, 0.12);
}

.login-auth-panel__card :deep(.el-card__body) {
  display: flex;
  flex: 1;
  flex-direction: column;
  min-height: 0;
  padding-top: 12px;
  position: relative;
  z-index: 1;
}

.login-auth-panel__header {
  display: grid;
  gap: 10px;
}

.login-auth-panel__eyebrow {
  display: inline-flex;
  width: fit-content;
  align-items: center;
  gap: 10px;
  color: rgba(191, 223, 255, 0.72);
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.14em;
  text-transform: uppercase;
  font-family: 'Fira Code', 'Consolas', monospace;
}

.login-auth-panel__eyebrow::before {
  width: 18px;
  height: 1px;
  background: rgba(108, 168, 255, 0.56);
  content: '';
}

.login-auth-panel__header strong {
  display: block;
  color: #f4f8ff;
  font-size: 28px;
  font-weight: 600;
  letter-spacing: -0.035em;
  font-family: 'Fira Sans', 'Segoe UI', 'PingFang SC', 'Microsoft YaHei', sans-serif;
}

.login-auth-panel__header p {
  margin: 0;
  color: rgba(210, 225, 245, 0.68);
  font-size: 14px;
  line-height: 1.75;
}

.login-auth-panel__tabs {
  display: flex;
  flex: 1;
  flex-direction: column;
  min-height: 0;
}

.login-auth-panel__tabs :deep(.el-tabs__header) {
  margin-bottom: 24px;
}

.login-auth-panel__tabs :deep(.el-tabs__nav) {
  gap: 18px;
}

.login-auth-panel__tabs :deep(.el-tabs__nav-wrap::after) {
  background-color: rgba(183, 216, 255, 0.1);
}

.login-auth-panel__tabs :deep(.el-tabs__item) {
  height: 42px;
  padding: 0;
  color: rgba(176, 193, 218, 0.82);
  font-weight: 600;
  transition: color 0.2s ease, text-shadow 0.2s ease;
}

.login-auth-panel__tabs :deep(.el-tabs__item.is-active) {
  color: #f4f8ff;
}

.login-auth-panel__tabs :deep(.el-tabs__active-bar) {
  height: 2px;
  background-color: #6ca8ff;
}

.login-auth-panel__tabs :deep(.el-tabs__content) {
  flex: 1;
  min-height: 0;
}

.login-auth-panel__tabs :deep(.el-tab-pane) {
  height: 100%;
}

.login-auth-panel__tab-content {
  display: flex;
  flex-direction: column;
  min-height: 316px;
}

.login-auth-panel__mode-copy {
  min-height: 20px;
  margin: 0 0 20px;
  color: rgba(192, 208, 230, 0.64);
  font-size: 13px;
  line-height: 1.7;
}

.login-auth-panel__form {
  display: flex;
  flex: 1;
  flex-direction: column;
}

.login-auth-panel__form :deep(.el-form-item) {
  margin-bottom: 20px;
}

.login-auth-panel__form :deep(.el-form-item__label) {
  color: rgba(229, 238, 251, 0.84);
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.02em;
}

.login-auth-panel__form :deep(.el-input__wrapper) {
  min-height: 48px;
  border-radius: 12px;
  box-shadow: 0 0 0 1px rgba(122, 176, 255, 0.12) inset;
  background: rgba(5, 15, 31, 0.72);
  transition: box-shadow 0.2s ease, background-color 0.2s ease;
}

.login-auth-panel__form :deep(.el-input__wrapper:hover) {
  box-shadow: 0 0 0 1px rgba(122, 176, 255, 0.18) inset;
}

.login-auth-panel__form :deep(.el-input__wrapper.is-focus) {
  box-shadow:
    0 0 0 1px rgba(108, 168, 255, 0.64) inset,
    0 0 0 3px rgba(108, 168, 255, 0.08);
}

.login-auth-panel__form :deep(.el-input__inner) {
  color: #f4f8ff;
}

.login-auth-panel__form :deep(.el-input__inner::placeholder) {
  color: rgba(163, 183, 211, 0.52);
}

.login-auth-panel__form :deep(.el-form-item__error) {
  color: #ffb4b4;
  font-size: 12px;
}

.login-auth-panel__verification-field {
  position: relative;
}

.login-auth-panel__verification-input :deep(.el-input__wrapper) {
  padding-right: 128px;
}

.login-auth-panel__submit {
  width: 100%;
  height: 52px;
  margin-top: auto;
  border: none;
  border-radius: 12px;
  background: linear-gradient(135deg, #1a49b0 0%, #3d72db 100%);
  box-shadow: 0 10px 24px rgba(25, 78, 187, 0.22);
  transition: background-color 0.2s ease;
}

.login-auth-panel__submit:hover,
.login-auth-panel__submit:focus-visible {
  background: linear-gradient(135deg, #2253bc 0%, #467ae0 100%);
}

.login-auth-panel__code-button {
  position: absolute;
  top: 50%;
  right: 6px;
  z-index: 2;
  min-width: 104px;
  height: 34px;
  padding: 0 14px;
  border: 1px solid rgba(122, 176, 255, 0.14);
  border-radius: 9px;
  background: linear-gradient(180deg, rgba(25, 49, 88, 0.96), rgba(14, 30, 58, 0.98));
  box-shadow:
    inset 0 1px 0 rgba(195, 223, 255, 0.08),
    0 6px 14px rgba(5, 12, 28, 0.2);
  color: rgba(226, 236, 251, 0.9);
  font-size: 12px;
  font-weight: 600;
  letter-spacing: 0.01em;
  transform: translateY(-50%);
  transition:
    border-color 0.2s ease,
    background-color 0.2s ease,
    color 0.2s ease,
    box-shadow 0.2s ease;
}

.login-auth-panel__code-button:hover {
  border-color: rgba(122, 176, 255, 0.22);
  background: linear-gradient(180deg, rgba(31, 58, 102, 0.98), rgba(18, 38, 72, 0.98));
  color: #f4f8ff;
  box-shadow:
    inset 0 1px 0 rgba(195, 223, 255, 0.1),
    0 8px 18px rgba(5, 12, 28, 0.24);
}

.login-auth-panel__code-button:focus-visible {
  border-color: rgba(108, 168, 255, 0.42);
  box-shadow:
    inset 0 1px 0 rgba(195, 223, 255, 0.1),
    0 0 0 3px rgba(108, 168, 255, 0.1);
}

.login-auth-panel__code-button.is-disabled,
.login-auth-panel__code-button:disabled {
  border-color: rgba(122, 176, 255, 0.1);
  background: linear-gradient(180deg, rgba(16, 31, 58, 0.78), rgba(11, 24, 48, 0.78));
  color: rgba(168, 188, 214, 0.46);
  box-shadow: none;
}

.login-auth-panel__footnote {
  margin-top: 20px;
  padding-top: 16px;
  border-top: 1px solid rgba(183, 216, 255, 0.1);
  color: rgba(168, 188, 214, 0.58);
  font-size: 12px;
  line-height: 1.7;
  text-align: center;
  font-family: 'Fira Code', 'Consolas', monospace;
}

@media (max-width: 1024px) {
  .login-auth-panel {
    justify-content: center;
    padding-right: 0;
  }
}

@media (max-width: 768px) {
  .login-auth-panel {
    align-items: flex-start;
    padding-right: 0;
  }

  .login-auth-panel__card {
    width: 100%;
    height: auto;
    min-height: 0;
    border-radius: 22px;
  }

  .login-auth-panel__header strong {
    font-size: 24px;
  }

  .login-auth-panel__tabs {
    min-height: auto;
  }

  .login-auth-panel__tabs :deep(.el-tabs__content) {
    min-height: auto;
  }

  .login-auth-panel__tabs :deep(.el-tab-pane) {
    height: auto;
  }

  .login-auth-panel__tab-content {
    min-height: 0;
  }

  .login-auth-panel__form {
    flex: initial;
  }

  .login-auth-panel__verification-input :deep(.el-input__wrapper) {
    padding-right: 118px;
  }

  .login-auth-panel__code-button {
    min-width: 96px;
    padding: 0 12px;
  }

  .login-auth-panel__submit {
    margin-top: 8px;
  }
}
</style>

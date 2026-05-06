import { computed, getCurrentScope, onScopeDispose, reactive, ref } from 'vue'

import * as authApi from '@/features/auth/api/authApi'
import {
  buildSendPhoneLoginCodePayload,
  createDefaultAccountLoginForm,
  createDefaultPhoneLoginForm,
  isValidAdminPhoneNumber,
  PHONE_LOGIN_CODE_BUTTON_TEXT,
  PHONE_LOGIN_CODE_COUNTDOWN_SECONDS,
  type LoginMode,
} from '@/features/auth/models/loginForms'

export function useLoginInteraction() {
  const activeMode = ref<LoginMode>('account')
  const accountFormModel = reactive(createDefaultAccountLoginForm())
  const phoneFormModel = reactive(createDefaultPhoneLoginForm())
  const isAccountSubmitting = ref(false)
  const isPhoneSubmitting = ref(false)
  const isSendingPhoneCode = ref(false)
  const phoneCodeCountdown = ref(0)
  let countdownTimer: ReturnType<typeof setInterval> | null = null

  const isSubmitting = computed(() => isAccountSubmitting.value || isPhoneSubmitting.value)
  const canSendPhoneCode = computed(
    () =>
      isValidAdminPhoneNumber(phoneFormModel.phoneNumber) &&
      !isSubmitting.value &&
      !isSendingPhoneCode.value &&
      phoneCodeCountdown.value === 0,
  )
  const phoneCodeButtonText = computed(() =>
    phoneCodeCountdown.value > 0
      ? `${phoneCodeCountdown.value}s 后重试`
      : PHONE_LOGIN_CODE_BUTTON_TEXT,
  )

  function setActiveMode(mode: LoginMode) {
    activeMode.value = mode
  }

  function clearPhoneCodeCountdown() {
    if (countdownTimer) {
      clearInterval(countdownTimer)
      countdownTimer = null
    }

    phoneCodeCountdown.value = 0
  }

  function startPhoneCodeCountdown(duration = PHONE_LOGIN_CODE_COUNTDOWN_SECONDS) {
    clearPhoneCodeCountdown()
    phoneCodeCountdown.value = duration

    countdownTimer = setInterval(() => {
      if (phoneCodeCountdown.value <= 1) {
        clearPhoneCodeCountdown()
        return
      }

      phoneCodeCountdown.value -= 1
    }, 1000)
  }

  async function withSubmitting<T>(
    state: typeof isAccountSubmitting,
    submitter: () => Promise<T>,
  ) {
    if (isSubmitting.value) {
      return null
    }

    state.value = true

    try {
      return await submitter()
    } finally {
      state.value = false
    }
  }

  async function sendPhoneCode() {
    if (!canSendPhoneCode.value) {
      return false
    }

    isSendingPhoneCode.value = true

    try {
      await authApi.sendPhoneLoginCode(buildSendPhoneLoginCodePayload(phoneFormModel))
      startPhoneCodeCountdown()
      return true
    } finally {
      isSendingPhoneCode.value = false
    }
  }

  async function runAccountLogin<T>(submitter: () => Promise<T>) {
    return withSubmitting(isAccountSubmitting, submitter)
  }

  async function runPhoneLogin<T>(submitter: () => Promise<T>) {
    return withSubmitting(isPhoneSubmitting, submitter)
  }

  if (getCurrentScope()) {
    onScopeDispose(clearPhoneCodeCountdown)
  }

  return {
    accountFormModel,
    activeMode,
    canSendPhoneCode,
    clearPhoneCodeCountdown,
    isAccountSubmitting,
    isPhoneSubmitting,
    isSendingPhoneCode,
    isSubmitting,
    phoneCodeButtonText,
    phoneCodeCountdown,
    phoneFormModel,
    runAccountLogin,
    runPhoneLogin,
    sendPhoneCode,
    setActiveMode,
    startPhoneCodeCountdown,
  }
}

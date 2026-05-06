<script setup lang="ts">
import { Lock } from '@element-plus/icons-vue'
import { useRouter } from 'vue-router'

const router = useRouter()

async function goBack() {
  await router.replace('/dashboard')
}
</script>

<template>
  <div class="forbidden-page app-page">
    <div class="forbidden-page__shell">
      <el-card class="forbidden-page__card" shadow="never">
        <div class="forbidden-page__badge">
          <el-icon><Lock /></el-icon>
          <span>Access Restricted</span>
        </div>

        <p class="forbidden-page__code">403</p>
        <h1 class="forbidden-page__title">你没有这个页面的访问权限</h1>
        <p class="forbidden-page__description">
          登录状态会保留，但当前路由所需的页面权限码不在你的授权上下文里。
        </p>

        <div class="forbidden-page__meta">
          <div class="forbidden-page__meta-item">
            <div class="forbidden-page__meta-label">状态</div>
            <div class="forbidden-page__meta-value">禁止访问</div>
          </div>
          <div class="forbidden-page__meta-item">
            <div class="forbidden-page__meta-label">建议操作</div>
            <div class="forbidden-page__meta-value">返回工作台或联系管理员分配权限</div>
          </div>
        </div>

        <div class="forbidden-page__actions">
          <el-button type="primary" class="forbidden-page__button" @click="goBack">
            返回仪表盘
          </el-button>
        </div>
      </el-card>
    </div>
  </div>
</template>

<style scoped>
.forbidden-page {
  display: grid;
  place-items: center;
  min-height: 100%;
  padding: 24px;
  background:
    radial-gradient(circle at top, rgba(59, 130, 246, 0.1), transparent 34%),
    linear-gradient(180deg, #f8fafc 0%, #f1f5f9 100%);
}

.forbidden-page__shell {
  width: min(680px, 100%);
}

.forbidden-page__card {
  border: 1px solid rgba(148, 163, 184, 0.24);
  border-radius: 24px;
  background: rgba(255, 255, 255, 0.96);
  box-shadow:
    0 20px 44px rgba(15, 23, 42, 0.08),
    0 8px 20px rgba(59, 130, 246, 0.08);
  text-align: center;
  backdrop-filter: blur(10px);
}

.forbidden-page__card :deep(.el-card__body) {
  padding: 36px 32px;
}

.forbidden-page__badge {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  min-height: 30px;
  margin-bottom: 16px;
  padding: 0 12px;
  border: 1px solid rgba(191, 219, 254, 0.9);
  border-radius: 999px;
  background: rgba(239, 246, 255, 0.92);
  color: #1d4ed8;
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.04em;
  text-transform: uppercase;
}

.forbidden-page__code {
  margin: 0 0 8px;
  color: #1e40af;
  font-size: clamp(44px, 8vw, 64px);
  font-weight: 800;
  line-height: 0.96;
  letter-spacing: -0.06em;
}

.forbidden-page__title {
  margin: 0 0 12px;
  color: #0f172a;
  font-size: clamp(24px, 3vw, 32px);
  font-weight: 760;
  line-height: 1.08;
  letter-spacing: -0.03em;
}

.forbidden-page__description {
  max-width: 520px;
  margin: 0 auto;
  color: #64748b;
  font-size: 14px;
  line-height: 1.7;
}

.forbidden-page__meta {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 12px;
  margin-top: 24px;
}

.forbidden-page__meta-item {
  padding: 14px 16px;
  border: 1px solid rgba(226, 232, 240, 0.95);
  border-radius: 16px;
  background: linear-gradient(180deg, rgba(248, 250, 252, 0.82), rgba(255, 255, 255, 0.98));
  text-align: left;
}

.forbidden-page__meta-label {
  color: #64748b;
  font-size: 11px;
  font-weight: 700;
  line-height: 1.35;
  letter-spacing: 0.06em;
  text-transform: uppercase;
}

.forbidden-page__meta-value {
  margin-top: 6px;
  color: #0f172a;
  font-size: 14px;
  font-weight: 600;
  line-height: 1.5;
}

.forbidden-page__actions {
  display: flex;
  justify-content: center;
  margin-top: 24px;
}

.forbidden-page__button {
  min-width: 148px;
  min-height: 42px;
  border-color: transparent;
  border-radius: 12px;
  background: linear-gradient(135deg, #1e40af 0%, #2563eb 100%);
  box-shadow: 0 10px 20px rgba(37, 99, 235, 0.18);
  font-weight: 700;
}

@media (max-width: 640px) {
  .forbidden-page {
    padding: 16px;
  }

  .forbidden-page__card :deep(.el-card__body) {
    padding: 28px 20px;
  }

  .forbidden-page__meta {
    grid-template-columns: 1fr;
  }

  .forbidden-page__button {
    width: 100%;
  }
}
</style>

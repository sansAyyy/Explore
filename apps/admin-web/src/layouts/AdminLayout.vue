<script setup lang="ts">
import { ElMessage } from 'element-plus'
import type { RouteRecordRaw } from 'vue-router'
import { computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'

import AdminSidebar from '@/layouts/admin-layout/AdminSidebar.vue'
import AdminTopbar from '@/layouts/admin-layout/AdminTopbar.vue'
import { useRouteLoading } from '@/router/loading'
import { useAdminLayout } from '@/layouts/admin-layout/useAdminLayout'
import {
  buildAdminNavigationFromRoutes,
  buildVisibleAdminNavigation,
} from '@/shared/constants/navigation'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
const route = useRoute()
const router = useRouter()
const isLoggingOut = ref(false)

const {
  closeMobileSidebar,
  isMobileSidebarOpen,
  isSidebarCollapsed,
  openMobileSidebar,
  toggleSidebarCollapsed,
} = useAdminLayout(computed(() => route.fullPath))

const navigationItems = computed(() =>
  buildAdminNavigationFromRoutes(router.options.routes as readonly RouteRecordRaw[]),
)
const visibleMenuItems = computed(() =>
  buildVisibleAdminNavigation(navigationItems.value, authStore.hasPagePermission),
)
const activeMenu = computed(() => String(route.meta.activeMenu || route.path))
const pageTitle = computed(() => String(route.meta.title || '管理后台'))
const displayName = computed(() => authStore.displayName || authStore.userName || '管理员')
const roleLabel = computed(() => authStore.roleCodes.join(' / ') || '未分配角色')
const isSyncingSession = computed(
  () => authStore.isAuthenticated && authStore.isBootstrappingSession,
)
const { isRouteNavigating } = useRouteLoading()

async function handleLogout() {
  isLoggingOut.value = true

  try {
    await authStore.logout()
    closeMobileSidebar()
    await router.replace('/login')
    ElMessage.success('已退出登录')
  } finally {
    isLoggingOut.value = false
  }
}

async function handleNavigateProfile() {
  closeMobileSidebar()
  await router.push({ name: 'profile' })
}

function handleNavigate() {
  closeMobileSidebar()
}
</script>

<template>
  <div
    class="admin-layout app-page"
    :style="{
      '--admin-sidebar-width': isSidebarCollapsed ? '80px' : '256px',
    }"
  >
    <aside class="admin-layout__sidebar-shell">
      <AdminSidebar
        :active-path="activeMenu"
        :collapsed="isSidebarCollapsed"
        :items="visibleMenuItems"
        @navigate="handleNavigate"
      />
    </aside>

    <el-drawer
      v-model="isMobileSidebarOpen"
      :with-header="false"
      append-to-body
      class="admin-layout__mobile-drawer"
      direction="ltr"
      size="280px"
    >
      <AdminSidebar
        :active-path="activeMenu"
        :items="visibleMenuItems"
        mobile
        @close="closeMobileSidebar"
        @navigate="handleNavigate"
      />
    </el-drawer>

    <div class="admin-layout__main">
      <AdminTopbar
        :collapsed="isSidebarCollapsed"
        :display-name="displayName"
        :is-logging-out="isLoggingOut"
        :is-syncing-session="isSyncingSession"
        :page-title="pageTitle"
        :role-label="roleLabel"
        @logout="handleLogout"
        @navigate-profile="handleNavigateProfile"
        @open-sidebar="openMobileSidebar"
        @toggle-sidebar="toggleSidebarCollapsed"
      />

      <transition name="admin-layout-progress">
        <div v-if="isRouteNavigating" class="admin-layout__progress" aria-hidden="true">
          <div class="admin-layout__progress-bar" />
        </div>
      </transition>

      <main class="admin-layout__content">
        <div class="admin-layout__content-shell">
          <RouterView />
        </div>
      </main>
    </div>
  </div>
</template>

<style scoped>
.admin-layout {
  display: grid;
  grid-template-columns: var(--admin-sidebar-width) minmax(0, 1fr);
  height: 100vh;
  min-height: 0;
  background:
    radial-gradient(circle at top left, rgba(97, 142, 255, 0.1), transparent 28%),
    linear-gradient(180deg, #edf3fb 0%, #f7f9fd 100%);
  transition: grid-template-columns 0.2s ease;
  overflow: hidden;
}

.admin-layout__sidebar-shell {
  min-width: 0;
  height: 100vh;
  min-height: 0;
  transition: width 0.2s ease;
}

.admin-layout__main {
  display: grid;
  grid-template-rows: auto minmax(0, 1fr);
  min-width: 0;
  height: 100vh;
  min-height: 0;
  overflow: hidden;
  position: relative;
}

.admin-layout__progress {
  position: absolute;
  top: 72px;
  right: 0;
  left: 0;
  z-index: 20;
  height: 3px;
  overflow: hidden;
  pointer-events: none;
}

.admin-layout__progress-bar {
  width: 40%;
  height: 100%;
  border-radius: 999px;
  background: linear-gradient(90deg, #4a6aff 0%, #6ca8ff 100%);
  box-shadow: 0 0 14px rgba(74, 106, 255, 0.35);
  animation: admin-layout-progress-slide 1.05s ease-in-out infinite;
}

.admin-layout__content {
  min-width: 0;
  min-height: 0;
  padding: 12px 16px 16px;
  overflow: hidden;
}

.admin-layout__content-shell {
  display: flex;
  flex-direction: column;
  min-height: 0;
  height: 100%;
  padding: 12px 16px;
  border: 1px solid rgba(125, 141, 170, 0.12);
  border-radius: 20px;
  background: rgba(255, 255, 255, 0.72);
  box-shadow: 0 18px 40px rgba(26, 42, 76, 0.08);
  backdrop-filter: blur(10px);
  overflow: hidden;
}

.admin-layout__mobile-drawer {
  display: none;
}

.admin-layout__mobile-drawer :deep(.el-drawer) {
  background: transparent;
  box-shadow: none;
}

.admin-layout__mobile-drawer :deep(.el-drawer__body) {
  height: 100%;
  padding: 0;
}

.admin-layout-progress-enter-active,
.admin-layout-progress-leave-active {
  transition: opacity 0.16s ease;
}

.admin-layout-progress-enter-from,
.admin-layout-progress-leave-to {
  opacity: 0;
}

@keyframes admin-layout-progress-slide {
  0% {
    transform: translateX(-120%);
  }

  100% {
    transform: translateX(320%);
  }
}

@media (max-width: 920px) {
  .admin-layout {
    grid-template-columns: 1fr;
    height: auto;
    min-height: 100vh;
    overflow: visible;
  }

  .admin-layout__sidebar-shell {
    display: none;
  }

  .admin-layout__main {
    height: auto;
    min-height: 100vh;
    overflow: visible;
  }

  .admin-layout__progress {
    top: 64px;
  }

  .admin-layout__mobile-drawer {
    display: block;
  }

  .admin-layout__content {
    padding: 16px;
    overflow: auto;
  }

  .admin-layout__content-shell {
    height: auto;
    min-height: 100%;
    padding: 16px;
    border-radius: 22px;
    overflow: visible;
  }
}

@media (max-width: 640px) {
  .admin-layout__content {
    padding: 12px;
  }

  .admin-layout__content-shell {
    padding: 14px;
    border-radius: 18px;
  }
}
</style>

<script setup lang="ts">
import type { AdminNavigationItem, AdminNavigationLinkItem } from '@/shared/constants/navigation'

import { Close } from '@element-plus/icons-vue'
import { computed } from 'vue'

import { isAdminNavigationGroup } from '@/shared/constants/navigation'

const props = withDefaults(
  defineProps<{
    activePath: string
    collapsed?: boolean
    items: AdminNavigationItem[]
    mobile?: boolean
  }>(),
  {
    collapsed: false,
    mobile: false,
  },
)

const emit = defineEmits<{
  close: []
  navigate: [path: string]
}>()

function isPathMatch(activePath: string, itemPath: string) {
  return activePath === itemPath || activePath.startsWith(`${itemPath}/`)
}

function getGroupIndex(title: string) {
  return `group:${title}`
}

function findActiveNavigationItem(items: AdminNavigationItem[], activePath: string): AdminNavigationLinkItem | null {
  for (const item of items) {
    if (isAdminNavigationGroup(item)) {
      const activeChild = item.children.find((child) => isPathMatch(activePath, child.path))
      if (activeChild) {
        return activeChild
      }

      continue
    }

    if (isPathMatch(activePath, item.path)) {
      return item
    }
  }

  return null
}

const resolvedActivePath = computed(() => findActiveNavigationItem(props.items, props.activePath)?.path ?? '')
const defaultOpenGroups = computed(() =>
  props.items
    .filter(
      (item) =>
        isAdminNavigationGroup(item) &&
        item.children.some((child) => isPathMatch(props.activePath, child.path)),
    )
    .map((item) => getGroupIndex(item.title)),
)

function handleSelect(index: string) {
  emit('navigate', index)
}

function handleClose() {
  emit('close')
}
</script>

<template>
  <aside
    class="admin-sidebar"
    :class="{
      'is-collapsed': collapsed && !mobile,
      'is-mobile': mobile,
    }"
  >
    <div class="admin-sidebar__header">
      <div class="admin-sidebar__brand">
        <span class="admin-sidebar__brand-mark">E</span>
        <transition name="admin-sidebar-fade">
          <div v-if="!collapsed || mobile" class="admin-sidebar__brand-copy">
            <strong>Explore Admin</strong>
            <p>管理端控制台</p>
          </div>
        </transition>
      </div>

      <el-button
        v-if="mobile"
        :icon="Close"
        class="admin-sidebar__close"
        text
        @click="handleClose"
      />
    </div>

    <el-scrollbar class="admin-sidebar__menu-scrollbar">
      <el-menu
        :collapse="collapsed && !mobile"
        :collapse-transition="false"
        :default-active="resolvedActivePath"
        :default-openeds="defaultOpenGroups"
        class="admin-sidebar__menu"
        router
        @select="handleSelect"
      >
        <template v-for="item in props.items" :key="item.type === 'group' ? item.title : item.path">
          <el-sub-menu
            v-if="item.type === 'group'"
            :index="getGroupIndex(item.title)"
          >
            <template #title>
              <el-icon v-if="item.icon">
                <component :is="item.icon" />
              </el-icon>
              <span>{{ item.title }}</span>
            </template>

            <el-menu-item
              v-for="child in item.children"
              :key="child.path"
              :index="child.path"
              :title="collapsed && !mobile ? child.title : undefined"
            >
              <el-icon v-if="child.icon">
                <component :is="child.icon" />
              </el-icon>
              <template #title>
                <span>{{ child.title }}</span>
              </template>
            </el-menu-item>
          </el-sub-menu>

          <el-menu-item
            v-else
            :index="item.path"
            :title="collapsed && !mobile ? item.title : undefined"
          >
            <el-icon v-if="item.icon">
              <component :is="item.icon" />
            </el-icon>
            <template #title>
              <span>{{ item.title }}</span>
            </template>
          </el-menu-item>
        </template>
      </el-menu>
    </el-scrollbar>
  </aside>
</template>

<style scoped>
.admin-sidebar {
  display: grid;
  grid-template-rows: auto minmax(0, 1fr);
  gap: 18px;
  height: 100%;
  padding: 20px 16px 18px;
  border-right: 1px solid rgba(255, 255, 255, 0.06);
  background:
    radial-gradient(circle at top left, rgba(108, 168, 255, 0.18) 0%, transparent 32%),
    linear-gradient(180deg, #122241 0%, #1a2c53 52%, #152644 100%);
  color: #f4f8ff;
}

.admin-sidebar.is-collapsed {
  grid-template-rows: auto minmax(0, 1fr);
  justify-items: center;
  padding-right: 12px;
  padding-left: 12px;
}

.admin-sidebar.is-mobile {
  border-right: none;
  border-radius: 0;
}

.admin-sidebar__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.admin-sidebar__brand {
  display: flex;
  align-items: center;
  gap: 12px;
  min-width: 0;
}

.admin-sidebar__brand-mark {
  display: inline-grid;
  place-items: center;
  width: 44px;
  height: 44px;
  flex: none;
  border-radius: 14px;
  background: linear-gradient(135deg, #6ca8ff 0%, #4a6aff 100%);
  box-shadow: 0 14px 24px rgba(74, 106, 255, 0.28);
  font-size: 18px;
  font-weight: 800;
}

.admin-sidebar__brand-copy {
  min-width: 0;
}

.admin-sidebar__brand-copy strong {
  display: block;
  font-size: 15px;
}

.admin-sidebar__brand-copy p {
  margin: 4px 0 0;
  color: rgba(244, 248, 255, 0.68);
  font-size: 12px;
}

.admin-sidebar__close {
  color: #f4f8ff;
}

.admin-sidebar__menu-scrollbar {
  min-height: 0;
}

.admin-sidebar__menu {
  border-right: none;
  background: transparent;
}

.admin-sidebar__menu :deep(.el-menu-item) {
  margin-bottom: 8px;
  border-radius: 14px;
  color: rgba(244, 248, 255, 0.8);
  transition: background-color 0.2s ease, color 0.2s ease;
}

.admin-sidebar__menu :deep(.el-sub-menu__title) {
  margin-bottom: 8px;
  border-radius: 14px;
  color: rgba(244, 248, 255, 0.8);
}

.admin-sidebar__menu :deep(.el-menu-item:hover) {
  color: #ffffff;
  background: rgba(255, 255, 255, 0.1);
}

.admin-sidebar__menu :deep(.el-sub-menu__title:hover) {
  color: #ffffff;
  background: rgba(255, 255, 255, 0.1);
}

.admin-sidebar__menu :deep(.el-menu-item.is-active) {
  color: #ffffff;
  background: linear-gradient(135deg, rgba(96, 143, 255, 0.28) 0%, rgba(54, 91, 214, 0.32) 100%);
  box-shadow: inset 0 0 0 1px rgba(133, 172, 255, 0.24);
}

.admin-sidebar__menu :deep(.el-sub-menu .el-menu-item) {
  min-width: 0;
  margin-bottom: 6px;
  margin-left: 10px;
}

.admin-sidebar__menu :deep(.el-sub-menu .el-menu) {
  background: transparent;
}

.admin-sidebar.is-collapsed .admin-sidebar__menu {
  --el-menu-base-level-padding: 16px;
  width: 56px;
}

.admin-sidebar.is-collapsed .admin-sidebar__menu :deep(.el-menu-item) {
  width: 56px;
  padding: 0;
  justify-content: center;
}

.admin-sidebar.is-collapsed .admin-sidebar__menu :deep(.el-menu-item > .el-icon) {
  margin-right: 0;
}

.admin-sidebar-fade-enter-active,
.admin-sidebar-fade-leave-active {
  transition: opacity 0.16s ease;
}

.admin-sidebar-fade-enter-from,
.admin-sidebar-fade-leave-to {
  opacity: 0;
}
</style>

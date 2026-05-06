<script setup lang="ts">
import { computed, onMounted } from 'vue'

import MessageTemplateUpsertDrawer from '@/features/message-templates/components/drawers/MessageTemplateUpsertDrawer.vue'
import MessageTemplatesFiltersCard from '@/features/message-templates/components/list/MessageTemplatesFiltersCard.vue'
import MessageTemplatesTable from '@/features/message-templates/components/list/MessageTemplatesTable.vue'
import { useMessageTemplateActivation } from '@/features/message-templates/composables/useMessageTemplateActivation'
import { useMessageTemplateDialogs } from '@/features/message-templates/composables/useMessageTemplateDialogs'
import { useMessageTemplatesList } from '@/features/message-templates/composables/useMessageTemplatesList'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
const list = useMessageTemplatesList()
const dialogs = useMessageTemplateDialogs({
  refreshTemplates: list.loadTemplates,
})
const activation = useMessageTemplateActivation({
  refreshTemplates: list.loadTemplates,
})

const canCreateMessageTemplate = computed(() =>
  authStore.hasButtonPermission('message_templates.create'),
)
const canUpdateMessageTemplate = computed(() =>
  authStore.hasButtonPermission('message_templates.update'),
)

onMounted(() => {
  void list.loadTemplates()
})
</script>

<template>
  <div class="message-templates-page">
    <section class="message-templates-page__hero">
      <p class="message-templates-page__eyebrow">Message Templates</p>
      <h1 class="message-templates-page__title">消息模板管理</h1>
    </section>

    <section class="message-templates-page__section">
      <MessageTemplatesFiltersCard
        :can-create-message-template="canCreateMessageTemplate"
        :filters="list.filters"
        :status-options="list.statusOptions"
        @create="dialogs.handleOpenCreateDrawer"
        @reset="list.handleReset"
        @search="list.handleSearch"
      />
    </section>

    <section class="message-templates-page__section message-templates-page__section--table">
      <MessageTemplatesTable
        :can-disable-row="activation.canDisableRow"
        :can-enable-row="activation.canEnableRow"
        :can-update-message-template="canUpdateMessageTemplate"
        :is-loading="list.isLoading.value"
        :is-row-activation-pending="activation.isRowActivationPending"
        :pagination="list.pagination"
        :templates="list.templates.value"
        :total-count="list.totalCount.value"
        @disable="activation.handleDisable"
        @edit="dialogs.handleOpenEditDrawer"
        @enable="activation.handleEnable"
        @page-change="list.handlePageChange"
        @page-size-change="list.handlePageSizeChange"
      />
    </section>

    <MessageTemplateUpsertDrawer
      v-model="dialogs.isUpsertDrawerOpen.value"
      :mode="dialogs.upsertMode.value"
      :template="dialogs.editingTemplate.value"
      @changed="dialogs.handleChanged"
    />
  </div>
</template>

<style scoped>
.message-templates-page {
  display: grid;
  grid-template-rows: auto auto minmax(0, 1fr);
  gap: 10px;
  min-height: 0;
  height: 100%;
  color: #334155;
  overflow: hidden;
}

.message-templates-page__hero {
  display: grid;
  gap: 2px;
  min-width: 0;
  padding: 0 2px 4px;
  border-bottom: 1px solid rgba(226, 232, 240, 0.9);
}

.message-templates-page__eyebrow {
  margin: 0;
  color: #64748b;
  font-size: 10px;
  font-weight: 700;
  letter-spacing: 0.12em;
  text-transform: uppercase;
}

.message-templates-page__title {
  margin: 0;
  color: #0f172a;
  font-size: clamp(22px, 2vw, 26px);
  font-weight: 750;
  line-height: 1.04;
  letter-spacing: -0.03em;
}

.message-templates-page__section {
  min-width: 0;
}

.message-templates-page__section--table {
  min-height: 0;
}

@media (max-width: 920px) {
  .message-templates-page {
    grid-template-rows: auto auto auto;
    gap: 12px;
    height: auto;
    overflow: visible;
  }

  .message-templates-page__hero {
    padding-bottom: 6px;
  }
}

@media (max-width: 640px) {
  .message-templates-page {
    gap: 10px;
  }

  .message-templates-page__title {
    font-size: clamp(20px, 7vw, 24px);
  }
}
</style>

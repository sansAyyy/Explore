import { ref, watch } from 'vue'

import type { MessageTemplateBasicResponse } from '@/features/message-templates/types/messageTemplates'

export function useMessageTemplateDialogs(options: {
  refreshTemplates: () => Promise<unknown> | void
}) {
  const isUpsertDrawerOpen = ref(false)
  const upsertMode = ref<'create' | 'edit'>('create')
  const editingTemplate = ref<MessageTemplateBasicResponse | null>(null)

  function handleOpenCreateDrawer() {
    upsertMode.value = 'create'
    editingTemplate.value = null
    isUpsertDrawerOpen.value = true
  }

  function handleOpenEditDrawer(row: MessageTemplateBasicResponse) {
    upsertMode.value = 'edit'
    editingTemplate.value = row
    isUpsertDrawerOpen.value = true
  }

  async function handleChanged() {
    await options.refreshTemplates()
  }

  watch(isUpsertDrawerOpen, (value) => {
    if (!value) {
      editingTemplate.value = null
      upsertMode.value = 'create'
    }
  })

  return {
    editingTemplate,
    handleChanged,
    handleOpenCreateDrawer,
    handleOpenEditDrawer,
    isUpsertDrawerOpen,
    upsertMode,
  }
}

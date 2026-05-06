import { describe, expect, it, vi } from 'vitest'

import { useMessageTemplateDialogs } from '@/features/message-templates/composables/useMessageTemplateDialogs'
import type { MessageTemplateBasicResponse } from '@/features/message-templates/types/messageTemplates'

const sampleTemplate: MessageTemplateBasicResponse = {
  id: 'template-1',
  code: 'order.pay_success.sms',
  name: 'Paid Notice',
  description: 'Notify user after order payment succeeds',
  isEnabled: true,
  channelType: 1,
}

describe('useMessageTemplateDialogs', () => {
  it('opens create drawer and clears editing template', () => {
    const dialogs = useMessageTemplateDialogs({
      refreshTemplates: vi.fn(),
    })

    dialogs.handleOpenCreateDrawer()

    expect(dialogs.isUpsertDrawerOpen.value).toBe(true)
    expect(dialogs.upsertMode.value).toBe('create')
    expect(dialogs.editingTemplate.value).toBeNull()
  })

  it('opens edit drawer with the selected template and refreshes on change', async () => {
    const refreshTemplates = vi.fn().mockResolvedValue(undefined)
    const dialogs = useMessageTemplateDialogs({
      refreshTemplates,
    })

    dialogs.handleOpenEditDrawer(sampleTemplate)

    expect(dialogs.isUpsertDrawerOpen.value).toBe(true)
    expect(dialogs.upsertMode.value).toBe('edit')
    expect(dialogs.editingTemplate.value).toEqual(sampleTemplate)

    await dialogs.handleChanged()
    expect(refreshTemplates).toHaveBeenCalledTimes(1)
  })
})

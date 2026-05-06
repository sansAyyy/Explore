import { buildServiceApiPath, gatewayServices } from '@/shared/api/gateway'
import { httpRequest } from '@/shared/api/http'

import type {
  CurrentCustomer,
  UpdateCurrentCustomerAvatarPayload,
  UpdateCurrentCustomerProfilePayload,
} from '@/features/customer-account/types/customerAccount'

const customerAccountApi = (path: string) => buildServiceApiPath(gatewayServices.customerAccount, path)

export const currentCustomerApi = {
  getCurrentCustomer() {
    return httpRequest<CurrentCustomer>({
      url: customerAccountApi('/api/customer-current-user'),
      method: 'GET',
    })
  },

  updateCurrentCustomerProfile(payload: UpdateCurrentCustomerProfilePayload) {
    return httpRequest<CurrentCustomer, UpdateCurrentCustomerProfilePayload>({
      url: customerAccountApi('/api/customer-current-user'),
      method: 'PUT',
      data: payload,
    })
  },

  updateCurrentCustomerAvatar(payload: UpdateCurrentCustomerAvatarPayload) {
    return httpRequest<CurrentCustomer, UpdateCurrentCustomerAvatarPayload>({
      url: customerAccountApi('/api/customer-current-user/avatar'),
      method: 'PUT',
      data: payload,
    })
  },
}

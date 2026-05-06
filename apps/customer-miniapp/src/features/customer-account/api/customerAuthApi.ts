import { buildServiceApiPath, gatewayServices } from '@/shared/api/gateway'
import { httpRequest } from '@/shared/api/http'

import type {
  CustomerLoginPayload,
  CustomerLoginResponse,
  CustomerLogoutPayload,
  CustomerRefreshTokenPayload,
  SendPhoneLoginCodePayload,
} from '@/features/customer-account/types/customerAccount'

const customerAccountApi = (path: string) => buildServiceApiPath(gatewayServices.customerAccount, path)

export const customerAuthApi = {
  sendPhoneLoginCode(payload: SendPhoneLoginCodePayload) {
    return httpRequest<void, SendPhoneLoginCodePayload>({
      url: customerAccountApi('/api/customer-auth/phone/code'),
      method: 'POST',
      data: payload,
      skipAuthToken: true,
      skipAuthRefresh: true,
    })
  },

  phoneLogin(payload: CustomerLoginPayload) {
    return httpRequest<CustomerLoginResponse, CustomerLoginPayload>({
      url: customerAccountApi('/api/customer-auth/phone/login'),
      method: 'POST',
      data: payload,
      skipAuthToken: true,
      skipAuthRefresh: true,
    })
  },

  refresh(payload: CustomerRefreshTokenPayload) {
    return httpRequest<CustomerLoginResponse, CustomerRefreshTokenPayload>({
      url: customerAccountApi('/api/customer-auth/refresh'),
      method: 'POST',
      data: payload,
      skipAuthToken: true,
      skipAuthRefresh: true,
    })
  },

  logout(payload: CustomerLogoutPayload) {
    return httpRequest<void, CustomerLogoutPayload>({
      url: customerAccountApi('/api/customer-auth/logout'),
      method: 'POST',
      data: payload,
      skipAuthRefresh: true,
    })
  },
}

import type {
  AdminPermissionDetailResponse,
  AdminPermissionFormModel,
  CreateAdminPermissionRequest,
  UpdateAdminPermissionRequest,
} from '@/features/admin-permissions/types/adminPermissions'

export function createDefaultAdminPermissionForm(): AdminPermissionFormModel {
  return {
    parentId: null,
    code: '',
    name: '',
    description: '',
    resourceType: 1,
    isActive: true,
  }
}

export function createAdminPermissionFormFromDetail(
  permission: Pick<
    AdminPermissionDetailResponse,
    'parentId' | 'code' | 'name' | 'description' | 'resourceType' | 'isActive'
  >,
): AdminPermissionFormModel {
  return {
    parentId: permission.parentId,
    code: permission.code,
    name: permission.name,
    description: permission.description ?? '',
    resourceType: permission.resourceType,
    isActive: permission.isActive,
  }
}

export function buildCreateAdminPermissionPayload(
  form: AdminPermissionFormModel,
): CreateAdminPermissionRequest {
  const description = form.description.trim()

  return {
    parentId: form.parentId,
    code: form.code.trim(),
    name: form.name.trim(),
    description: description ? description : null,
    resourceType: form.resourceType,
    isActive: form.isActive,
  }
}

export function buildUpdateAdminPermissionPayload(
  form: AdminPermissionFormModel,
): UpdateAdminPermissionRequest {
  const payload = buildCreateAdminPermissionPayload(form)

  return {
    parentId: payload.parentId,
    code: payload.code,
    name: payload.name,
    description: payload.description,
    resourceType: payload.resourceType,
  }
}

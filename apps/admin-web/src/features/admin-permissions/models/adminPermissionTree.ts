import type {
  AdminPermissionBasicResponse,
  AdminPermissionResourceType,
  AdminPermissionTreeFilterState,
  AdminPermissionTreeNode,
  AdminPermissionTreeNodeResponse,
} from '@/features/admin-permissions/types/adminPermissions'

export function isAdminPermissionGroupNode(
  node: Pick<AdminPermissionBasicResponse, 'resourceType'>,
) {
  return node.resourceType === 3
}

export function getAdminPermissionResourceTypeLabel(resourceType: AdminPermissionResourceType) {
  switch (resourceType) {
    case 1:
      return '页面权限'
    case 2:
      return '按钮权限'
    case 3:
      return '分组节点'
    default:
      return '未知类型'
  }
}

export function createDefaultAdminPermissionTreeFilters(): AdminPermissionTreeFilterState {
  return {
    keyword: '',
    resourceType: 'all',
    status: 'all',
    onlySelected: false,
  }
}

export function mapAdminPermissionTreeNode(
  node: AdminPermissionTreeNodeResponse | AdminPermissionBasicResponse,
): AdminPermissionTreeNode {
  const children = 'children' in node ? node.children : []
  const isGroup = isAdminPermissionGroupNode(node)

  return {
    id: node.id,
    parentId: node.parentId,
    code: node.code,
    name: node.name,
    description: node.description,
    resourceType: node.resourceType,
    isGroup,
    resourceTypeLabel: getAdminPermissionResourceTypeLabel(node.resourceType),
    isActive: node.isActive,
    children: children.map((child) => mapAdminPermissionTreeNode(child)),
  }
}

export function flattenAdminPermissionTree(nodes: AdminPermissionTreeNode[]) {
  const result: AdminPermissionTreeNode[] = []

  function walk(list: AdminPermissionTreeNode[]) {
    for (const node of list) {
      result.push(node)
      if (node.children.length > 0) {
        walk(node.children)
      }
    }
  }

  walk(nodes)
  return result
}

export function collectAdminPermissionIds(nodes: AdminPermissionTreeNode[]) {
  return flattenAdminPermissionTree(nodes).map((node) => node.id)
}

export function buildRoleSelectablePermissionTree(nodes: AdminPermissionTreeNode[]): AdminPermissionTreeNode[] {
  return nodes.map((node) => ({
    ...node,
    disabled: false,
    children: buildRoleSelectablePermissionTree(node.children),
  }))
}

function matchesStatus(node: AdminPermissionTreeNode, status: AdminPermissionTreeFilterState['status']) {
  if (status === 'enabled') {
    return node.isActive
  }

  if (status === 'disabled') {
    return !node.isActive
  }

  return true
}

function matchesResourceType(
  node: AdminPermissionTreeNode,
  resourceType: AdminPermissionTreeFilterState['resourceType'],
) {
  if (resourceType === 'all') {
    return true
  }

  return node.resourceType === resourceType
}

function matchesKeyword(node: AdminPermissionTreeNode, keyword: string) {
  if (!keyword) {
    return true
  }

  const normalizedKeyword = keyword.trim().toLowerCase()
  if (!normalizedKeyword) {
    return true
  }

  return [node.code, node.name, node.description ?? ''].some((value) =>
    value.toLowerCase().includes(normalizedKeyword),
  )
}

export function filterAdminPermissionTree(
  nodes: AdminPermissionTreeNode[],
  filters: AdminPermissionTreeFilterState,
  selectedIds: string[],
) {
  const selectedSet = new Set(selectedIds)
  const keyword = filters.keyword.trim()

  function filterNode(node: AdminPermissionTreeNode): AdminPermissionTreeNode | null {
    const filteredChildren = node.children
      .map(filterNode)
      .filter((child): child is AdminPermissionTreeNode => child !== null)

    const matchesCurrentNode =
      matchesKeyword(node, keyword) &&
      matchesResourceType(node, filters.resourceType) &&
      matchesStatus(node, filters.status) &&
      (!filters.onlySelected || selectedSet.has(node.id))

    if (!matchesCurrentNode && filteredChildren.length === 0) {
      return null
    }

    return {
      ...node,
      children: filteredChildren,
    }
  }

  return nodes
    .map(filterNode)
    .filter((node): node is AdminPermissionTreeNode => node !== null)
}

export function buildParentSelectableTree(
  nodes: AdminPermissionTreeNode[],
  excludedIds: string[] = [],
): AdminPermissionTreeNode[] {
  const excludedSet = new Set(excludedIds)

  return nodes.map((node) => ({
    ...node,
    disabled: excludedSet.has(node.id),
    children: buildParentSelectableTree(node.children, excludedIds),
  }))
}

export function collectAdminPermissionSubtreeIds(node: AdminPermissionTreeNode) {
  return collectAdminPermissionIds([node])
}

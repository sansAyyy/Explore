import { describe, expect, it } from 'vitest'

import {
  buildRoleSelectablePermissionTree,
  collectAdminPermissionIds,
  collectAdminPermissionSubtreeIds,
  createDefaultAdminPermissionTreeFilters,
  filterAdminPermissionTree,
  flattenAdminPermissionTree,
  getAdminPermissionResourceTypeLabel,
  mapAdminPermissionTreeNode,
} from '@/features/admin-permissions/models/adminPermissionTree'

const tree = [
  {
    id: 'group-1',
    parentId: null,
    code: 'system_management.page',
    name: '系统管理',
    description: null,
    resourceType: 3 as const,
    isActive: true,
    children: [
      {
        id: 'root-1',
        parentId: 'group-1',
        code: 'admin_users.page',
        name: '用户管理',
        description: null,
        resourceType: 1 as const,
        isActive: true,
        children: [
          {
            id: 'child-1',
            parentId: 'root-1',
            code: 'admin_users.create',
            name: '新增用户管理',
            description: null,
            resourceType: 2 as const,
            isActive: true,
            children: [],
          },
        ],
      },
    ],
  },
]

describe('admin permission tree model', () => {
  it('maps tree nodes and resolves group labels', () => {
    expect(getAdminPermissionResourceTypeLabel(3)).toBe('分组节点')
    expect(mapAdminPermissionTreeNode(tree[0]).resourceTypeLabel).toBe('分组节点')
    expect(mapAdminPermissionTreeNode(tree[0]).children[0].children[0].resourceTypeLabel).toBe(
      '按钮权限',
    )
  })

  it('creates default filters and flattens tree nodes', () => {
    expect(createDefaultAdminPermissionTreeFilters()).toEqual({
      keyword: '',
      resourceType: 'all',
      status: 'all',
      onlySelected: false,
    })

    expect(flattenAdminPermissionTree(tree.map(mapAdminPermissionTreeNode))).toHaveLength(3)
    expect(collectAdminPermissionIds(tree.map(mapAdminPermissionTreeNode))).toEqual([
      'group-1',
      'root-1',
      'child-1',
    ])
  })

  it('filters tree by selected nodes and collects subtree ids', () => {
    const mappedTree = tree.map(mapAdminPermissionTreeNode)

    expect(
      filterAdminPermissionTree(
        mappedTree,
        {
          keyword: '',
          resourceType: 'all',
          status: 'all',
          onlySelected: true,
        },
        ['child-1'],
      ),
    ).toHaveLength(1)

    expect(collectAdminPermissionSubtreeIds(mappedTree[0])).toEqual(['group-1', 'root-1', 'child-1'])
  })

  it('filters tree by resource type while preserving matching branches', () => {
    const mappedTree = tree.map(mapAdminPermissionTreeNode)

    const filteredTree = filterAdminPermissionTree(
      mappedTree,
      {
        keyword: '',
        resourceType: 3,
        status: 'all',
        onlySelected: false,
      },
      [],
    )

    expect(filteredTree).toHaveLength(1)
    expect(filteredTree[0].id).toBe('group-1')
    expect(filteredTree[0].children).toHaveLength(0)
  })

  it('allows selecting group nodes in the role permissions page tree', () => {
    const mappedTree = tree.map(mapAdminPermissionTreeNode)
    const selectableTree = buildRoleSelectablePermissionTree(mappedTree)

    expect(selectableTree[0].disabled).toBe(false)
    expect(selectableTree[0].children[0].disabled).toBe(false)
    expect(selectableTree[0].children[0].children[0].disabled).toBe(false)
  })
})

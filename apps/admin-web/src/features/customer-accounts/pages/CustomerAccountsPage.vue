<script setup lang="ts">
import { computed, onMounted } from 'vue'

import CustomerAccountDetailDrawer from '@/features/customer-accounts/components/drawers/CustomerAccountDetailDrawer.vue'
import CustomerAccountsFiltersCard from '@/features/customer-accounts/components/list/CustomerAccountsFiltersCard.vue'
import CustomerAccountsTable from '@/features/customer-accounts/components/list/CustomerAccountsTable.vue'
import { useCustomerAccountActivation } from '@/features/customer-accounts/composables/useCustomerAccountActivation'
import { useCustomerAccountDetailDrawer } from '@/features/customer-accounts/composables/useCustomerAccountDetailDrawer'
import { useCustomerAccountsList } from '@/features/customer-accounts/composables/useCustomerAccountsList'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
const list = useCustomerAccountsList()
const detailDrawer = useCustomerAccountDetailDrawer()
const activation = useCustomerAccountActivation({
  refreshCustomers: list.loadCustomers,
})

const canUpdateCustomerAccount = computed(() =>
  authStore.hasButtonPermission('customer_accounts.update'),
)

onMounted(() => {
  void list.loadCustomers()
})
</script>

<template>
  <div class="customer-accounts-page">
    <section class="customer-accounts-page__hero">
      <p class="customer-accounts-page__eyebrow">Customer Admin</p>
      <h1 class="customer-accounts-page__title">客户管理</h1>
    </section>

    <section class="customer-accounts-page__section">
      <CustomerAccountsFiltersCard
        :filters="list.filters"
        :status-options="list.statusOptions"
        @reset="list.handleReset"
        @search="list.handleSearch"
      />
    </section>

    <section class="customer-accounts-page__section customer-accounts-page__section--table">
      <CustomerAccountsTable
        :can-disable-row="activation.canDisableRow"
        :can-enable-row="activation.canEnableRow"
        :can-update-customer-account="canUpdateCustomerAccount"
        :customers="list.customers.value"
        :is-loading="list.isLoading.value"
        :is-row-activation-pending="activation.isRowActivationPending"
        :pagination="list.pagination"
        :total-count="list.totalCount.value"
        @detail="detailDrawer.handleOpenDetailDrawer"
        @disable="activation.handleDisable"
        @enable="activation.handleEnable"
        @page-change="list.handlePageChange"
        @page-size-change="list.handlePageSizeChange"
      />
    </section>

    <CustomerAccountDetailDrawer
      v-model="detailDrawer.isDetailDrawerOpen.value"
      :customer="detailDrawer.selectedCustomer.value"
      :detail="detailDrawer.detail.value"
      :is-loading="detailDrawer.isLoading.value"
    />
  </div>
</template>

<style scoped>
.customer-accounts-page {
  display: grid;
  grid-template-rows: auto auto minmax(0, 1fr);
  gap: 10px;
  min-height: 0;
  height: 100%;
  color: #334155;
  overflow: hidden;
}

.customer-accounts-page__hero {
  display: grid;
  gap: 2px;
  min-width: 0;
  padding: 0 2px 4px;
  border-bottom: 1px solid rgba(226, 232, 240, 0.9);
}

.customer-accounts-page__eyebrow {
  margin: 0;
  color: #64748b;
  font-size: 10px;
  font-weight: 700;
  letter-spacing: 0.12em;
  text-transform: uppercase;
}

.customer-accounts-page__title {
  margin: 0;
  color: #0f172a;
  font-size: clamp(22px, 2vw, 26px);
  font-weight: 750;
  line-height: 1.04;
  letter-spacing: -0.03em;
}

.customer-accounts-page__section {
  min-width: 0;
}

.customer-accounts-page__section--table {
  min-height: 0;
}

@media (max-width: 920px) {
  .customer-accounts-page {
    grid-template-rows: auto auto auto;
    gap: 12px;
    height: auto;
    overflow: visible;
  }

  .customer-accounts-page__hero {
    padding-bottom: 6px;
  }
}

@media (max-width: 640px) {
  .customer-accounts-page {
    gap: 10px;
  }

  .customer-accounts-page__title {
    font-size: clamp(20px, 7vw, 24px);
  }
}
</style>

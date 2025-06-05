<template>
  <ResponsiveLayout>
    <div
      class="flex-1 lg:flex-none flex flex-col gap-[1.125rem] p-[1.125rem] rounded-none lg:rounded-md bg-surface-0"
    >
      <h3 class="page-title mb-2">{{ l.navigation.manageRuleViolationReports }}</h3>

      <Tabs value="0" class="flex-1 flex flex-col">
        <TabList>
          <Tab value="0">{{ l.manageRuleViolationReports.unresolved }}</Tab>
          <Tab value="1">{{ l.manageRuleViolationReports.resolved }}</Tab>
        </TabList>
        <TabPanels class="px-0 pb-0 flex-1 flex flex-col">
          <TabPanel value="0" class="flex-1 flex flex-col">
            <RuleViolationTable :showResolved="false" />
          </TabPanel>
          <TabPanel value="1" class="flex-1 flex flex-col">
            <RuleViolationTable :showResolved="true" />
          </TabPanel>
        </TabPanels>
      </Tabs>
    </div>
  </ResponsiveLayout>
</template>

<script lang="ts" setup>
import ResponsiveLayout from '@/components/common/ResponsiveLayout.vue'
import RuleViolationTable from '@/components/rule-violation-report/RuleViolationTable.vue'
import { LocaleService } from '@/services/locale-service'

const l = LocaleService.currentLocale
</script>

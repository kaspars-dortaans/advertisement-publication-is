<template>
  <ResponsivePanel :title="l.navigation.manageRuleViolationReports">
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
  </ResponsivePanel>
</template>

<script lang="ts" setup>
import ResponsivePanel from '@/components/common/ResponsivePanel.vue'
import RuleViolationTable from '@/components/rule-violation-report/RuleViolationTable.vue'
import { LocaleService } from '@/services/locale-service'

const l = LocaleService.currentLocale
</script>

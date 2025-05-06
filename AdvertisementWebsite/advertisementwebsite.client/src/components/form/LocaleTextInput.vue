<template>
  <Tabs :value="currentTab">
    <TabList>
      <Tab v-for="(locale, i) in localeList" :key="locale" :value="i">{{ locale }}</Tab>
    </TabList>
    <TabPanels class="px-0 pb-0">
      <TabPanel v-for="(locale, i) in localeList" :key="locale" :value="i">
        <FloatLabel variant="on">
          <InputText
            v-model="model[i]"
            :invalid="invalid"
            :id="'localized-text-input-' + i"
            fluid
          />
          <label :for="'localized-text-input-' + i">{{ locale }} {{ label }}</label>
        </FloatLabel>
      </TabPanel>
    </TabPanels>
  </Tabs>
</template>

<script lang="ts" setup>
import { LocaleService } from '@/services/locale-service'
import { onBeforeMount, ref } from 'vue'

const props = defineProps<{
  localeList: string[]
  invalid?: boolean
  label: string
}>()
const currentTab = ref(0)
const model = defineModel<string[]>({
  default: []
})

onBeforeMount(() => {
  const currentLocaleIndex = props.localeList.indexOf(LocaleService.currentLocaleName.value)
  if (currentLocaleIndex > -1) {
    currentTab.value = currentLocaleIndex
  }
})
</script>

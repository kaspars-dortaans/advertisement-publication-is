<template>
  <ResponsiveLayout>
    <BlockWithSpinner :loading="loading" class="flex-1 lg:flex-none flex flex-col">
      <Panel class="rounded-none lg:rounded-md flex-1 lg:min-w-96">
        <template #header>
          <div class="panel-title-container">
            <BackButton :defaultTo="{ name: 'manageAttributes' }" />
            <h4 class="page-title">
              {{ l.navigation.viewAttribute }}
            </h4>
            <Button
              :label="l.actions.edit"
              icon="pi pi-pencil"
              severity="secondary"
              as="RouterLink"
              :to="{ name: 'editAttribute', params: { attributeId } }"
            />
          </div>
        </template>

        <div class="flex flex-col gap-4">
          <!-- Localized names -->
          <LocaleTextInput
            v-model="localizedNames"
            :localeList="ls.localeList.value"
            :label="l.form.attributeForm.title"
            disabled
          />

          <!-- Value type -->
          <FloatLabel variant="on">
            <InputText v-model="attributeData.valueType" id="value-type-input" fluid disabled />
            <label for="value-type-input">{{ l.form.attributeForm.valueType }}</label>
          </FloatLabel>

          <!-- Attribute value list -->
          <template v-if="valueListType">
            <FloatLabel variant="on">
              <InputText
                v-model="attributeData.attributeValueListName"
                id="attribute-value-list-input"
                fluid
                disabled
              />
              <label for="attribute-value-list-input">{{
                l.form.attributeForm.attributeValueList
              }}</label>
            </FloatLabel>
          </template>

          <!-- Filter type -->
          <FloatLabel variant="on">
            <InputText v-model="attributeData.filterType" id="filter-type-input" fluid disabled />
            <label for="filter-type-input">{{ l.form.attributeForm.filterType }}</label>
          </FloatLabel>

          <!-- sortable -->
          <div class="inline-flex gap-2 mt-4">
            <ToggleSwitch v-model="attributeData.sortable" id="is-sortable-input" disabled />
            <label for="is-sortable-input">{{ l.form.attributeForm.sortable }}</label>
          </div>

          <!-- searchable -->
          <div class="inline-flex gap-2 mt-4">
            <ToggleSwitch v-model="attributeData.searchable" id="is-searchable-input" disabled />
            <label for="is-searchable-input">{{ l.form.attributeForm.searchable }}</label>
          </div>

          <!-- showOnListItem -->
          <div class="inline-flex gap-2 mt-4">
            <ToggleSwitch
              v-model="attributeData.showOnListItem"
              id="show-on-list-item-input"
              disabled
            />
            <label for="show-on-list-item-input">{{ l.form.attributeForm.showOnListItem }}</label>
          </div>

          <FloatLabel variant="on">
            <InputText v-model="attributeData.iconName" id="icon-name-input" fluid disabled />
            <label for="icon-name-input">{{ l.form.attributeForm.iconName }}</label>
          </FloatLabel>
        </div>
      </Panel>
    </BlockWithSpinner>
  </ResponsiveLayout>
</template>

<script lang="ts" setup>
import BackButton from '@/components/common/BackButton.vue'
import BlockWithSpinner from '@/components/common/BlockWithSpinner.vue'
import ResponsiveLayout from '@/components/common/ResponsiveLayout.vue'
import LocaleTextInput from '@/components/form/LocaleTextInput.vue'
import { AttributeClient, FilterType, PutAttributeRequest, ValueTypes } from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { computed, onBeforeMount, ref, watch } from 'vue'

const props = defineProps<{
  attributeId?: number
}>()

//Services
const l = LocaleService.currentLocale
const ls = LocaleService.get()
const attributeService = getClient(AttributeClient)

//Reactive data
const loading = ref(0)
const localizedNames = ref<string[]>([])
const attributeData = ref<PutAttributeRequest>(
  new PutAttributeRequest({
    valueType: ValueTypes.Text,
    filterType: FilterType.Search,
    sortable: false,
    searchable: false,
    showOnListItem: false,
    localizedNames: []
  })
)
const valueListType = computed<boolean>(
  () => attributeData.value.valueType === ValueTypes.ValueListEntry
)

//Hooks
onBeforeMount(async () => {
  reloadData()
})

//Watchers
watch(LocaleService.currentLocaleName, () => reloadData())

//Methods
const reloadData = async () => {
  loading.value++
  const formInfo = await attributeService.getAttributeFormInfo(props.attributeId)
  localizedNames.value = ls.localeList.value.map(
    (l) => formInfo.localizedNames.find((ln) => ln.key === l)?.value ?? ''
  )
  attributeData.value = formInfo
  loading.value--
}
</script>

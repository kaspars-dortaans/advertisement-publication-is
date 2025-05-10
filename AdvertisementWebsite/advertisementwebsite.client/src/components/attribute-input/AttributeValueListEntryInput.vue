<template>
  <DataTable
    :value="tableRows"
    v-model:editingRows="editingRows"
    :editMode="disabled ? undefined : 'row'"
    :reorderableColumns="!disabled"
    :class="{ 'p-invalid': invalid }"
    @rowEditCancel="cancelRowChanges"
    @rowEditSave="saveRowChanges"
    @rowReorder="reorderEntries"
  >
    <template #header>
      <div class="flex justify-between items-center">
        <h4 class="font-semibold">{{ l.form.attributeValueListForm.entries }}</h4>
        <Button v-if="!disabled" :label="l.actions.add" @click="addNewRow" />
      </div>
    </template>

    <Column v-if="!disabled" headerStyle="width: 1rem" rowReorder />

    <Column v-for="locale in ls.localeList.value" :key="locale" :field="locale" :header="locale">
      <template #editor="{ data, field }">
        <InputText v-model="data[field]" fluid />
      </template>
    </Column>

    <Column :rowEditor="true" style="width: 1rem; padding-right: 0"></Column>
    <Column v-if="!disabled" style="width: 1rem; padding-left: 0">
      <template #body="slotProps">
        <Button
          icon="pi pi-trash"
          severity="danger"
          variant="text"
          rounded
          @click="removeRow(slotProps.index)"
        ></Button>
      </template>
    </Column>
  </DataTable>
</template>

<script lang="ts" setup>
import { AttributeValueListEntryDto, StringStringKeyValuePair } from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import {
  type DataTableRowEditCancelEvent,
  type DataTableRowEditSaveEvent,
  type DataTableRowReorderEvent
} from 'primevue'
import { ref, watch } from 'vue'

type tableRowType = { [k: string]: string | number | undefined }
const model = defineModel<AttributeValueListEntryDto[]>()
defineProps<{
  disabled?: boolean
  invalid?: boolean
}>()

//Services
const ls = LocaleService.get()
const l = LocaleService.currentLocale

//Reactive data
const editingRows = ref<tableRowType[]>([])
const tableRows = ref<tableRowType[]>([])

//Watchers
watch(
  model,
  () => {
    tableRows.value = (model.value ?? []).map((e) => {
      let row: tableRowType = { id: e.id }
      for (const locale of ls.localeList.value) {
        const localeName = e.localizedNames?.find((ln) => ln.key == locale)
        row[locale] = localeName?.value ?? ''
      }
      return row
    })
  },
  { immediate: true, deep: true }
)

//Methods
const addNewRow = () => {
  const newRow: tableRowType = getEmptyTableRow()
  tableRows.value.push(newRow)
  editingRows.value = [tableRows.value[tableRows.value.length - 1]]
}

const getEmptyTableRow = () => {
  const newRow: tableRowType = {}
  for (const locale of ls.localeList.value) {
    newRow[locale] = ''
  }
  return newRow
}

const removeRow = (i: number) => {
  model.value!.splice(i, 1)
}

const saveRowChanges = (e: DataTableRowEditSaveEvent) => {
  const newData = e.newData as tableRowType

  if (model.value!.length <= e.index) {
    model.value!.push(
      new AttributeValueListEntryDto({
        orderIndex: model.value!.length,
        localizedNames: []
      })
    )
  }
  const entry = model.value![e.index]
  for (const locale of ls.localeList.value) {
    let l = entry.localizedNames?.find((p) => p.key === locale)
    if (!l) {
      entry.localizedNames!.push(
        new StringStringKeyValuePair({
          key: locale,
          value: newData[locale] as string
        })
      )
    } else {
      l.value = newData[locale] as string
    }
  }
}

const cancelRowChanges = (e: DataTableRowEditCancelEvent) => {
  //If canceled on new table row remove it
  if (model.value && e.index >= model.value.length) {
    tableRows.value.splice(e.index)
  }
}

const reorderEntries = (e: DataTableRowReorderEvent) => {
  if (e.dragIndex == e.dropIndex || !model.value) {
    return
  }

  const entryToMove = model.value[e.dragIndex]
  const delta = e.dragIndex < e.dropIndex ? 1 : -1
  for (let i = e.dragIndex; i !== e.dropIndex; i += delta) {
    model.value[i] = model.value[i + delta]
    model.value[i].orderIndex = i
  }
  model.value[e.dropIndex] = entryToMove
  model.value[e.dropIndex].orderIndex = e.dropIndex
}
</script>

<template>
  <ResponsiveLayout>
    <BlockWithSpinner :loading="loadingCategories" class="flex-1 lg:flex-none flex flex-col">
      <Tree
        ref="tree"
        v-model:selectionKeys="selectedCategoryNodes"
        :value="categoryNodes"
        :pt="{ nodelabel: 'inline-flex items-center gap-2 justify-between flex-1' }"
        class="flex-1 lg:min-w-72 lg:min-h-96 rounded-none lg:rounded-md"
      >
        <template #header>
          <h4 class="page-title mb-2">{{ l.manageCategories.manageCategories }}</h4>
        </template>
        <template #default="slotProps">
          <span>{{ slotProps.node.label }}</span>
          <span :class="{ 'bg-surface-100 rounded-md': slotProps.selected }" @click.stop>
            <Button
              v-if="isAllowedToCreate"
              icon="pi pi-plus-circle"
              variant="text"
              as="RouterLink"
              :to="{ name: 'createCategory', params: { parentCategoryId: slotProps.node.key } }"
            />
            <Button
              icon="pi pi-eye"
              variant="text"
              as="RouterLink"
              :to="{ name: 'viewCategory', params: { categoryId: slotProps.node.key } }"
            />
            <Button
              v-if="isAllowedToEdit"
              icon="pi pi-pencil"
              variant="text"
              severity="secondary"
              as="RouterLink"
              :to="{ name: 'editCategory', params: { categoryId: slotProps.node.key } }"
            />
            <Button
              v-if="isAllowedToDelete"
              icon="pi pi-trash"
              variant="text"
              severity="danger"
              @click="confirmDeleteCategory(slotProps.node)"
            />
          </span>
        </template>
        <template #footer>
          <Button
            :label="l.actions.create"
            icon="pi pi-plus-circle"
            variant="outlined"
            class="mt-2"
            as="RouterLink"
            :to="{ name: 'createCategory' }"
          />
        </template>
      </Tree>
    </BlockWithSpinner>
  </ResponsiveLayout>
</template>

<script lang="ts" setup>
import ResponsiveLayout from '@/components/common/ResponsiveLayout.vue'
import BlockWithSpinner from '@/components/common/BlockWithSpinner.vue'
import { computed, onBeforeMount, ref, watch } from 'vue'
import { type TreeNode } from 'primevue/treenode'
import { getClient } from '@/utils/client-builder'
import { CategoryClient, CategoryItem } from '@/services/api-client'
import { buildNodeHierarchy } from '@/utils/build-node-hierarchy'
import { LocaleService } from '@/services/locale-service'
import { useConfirm } from 'primevue'
import { confirmDelete } from '@/utils/confirm-dialog'
import { AuthService } from '@/services/auth-service'
import { Permissions } from '@/constants/api/Permissions'

//Services
const categoryService = getClient(CategoryClient)
const l = LocaleService.currentLocale
const ls = LocaleService.get()
const confirm = useConfirm()

//Reactive data
const loadingCategories = ref(false)
const categoryNodes = ref<TreeNode[]>([])
const selectedCategoryNodes = ref({})
const isAllowedToCreate = computed(() => AuthService.hasPermission(Permissions.CreateCategory))
const isAllowedToEdit = computed(() => AuthService.hasPermission(Permissions.EditCategory))
const isAllowedToDelete = computed(() => AuthService.hasPermission(Permissions.DeleteCategory))

//Hooks
onBeforeMount(() => {
  loadCategories()
})

//Watchers
watch(LocaleService.currentLocaleName, () => {
  loadCategories()
})

//Methods
const loadCategories = async () => {
  loadingCategories.value = true
  const categories = await categoryService.getCategories()
  categoryNodes.value = buildNodeHierarchy<CategoryItem, TreeNode>(
    categories,
    'id',
    'parentCategoryId',
    (category, children) => ({
      key: '' + category.id,
      label: category.name,
      children,
      leaf: !children.length,
      data: {
        parent: null
      }
    }),
    (node) => {
      if (!node.children?.length) {
        return
      }

      for (const child of node.children) {
        child.data.parent = node
      }
    }
  )
  loadingCategories.value = false
}

const confirmDeleteCategory = (node: TreeNode) => {
  //Count subcategories
  const stack = node.children ? [...node.children] : []
  let total = stack.length
  let childNode = stack.pop()
  while (childNode) {
    if (childNode.children) {
      total += childNode.children.length
      stack.push(...childNode.children)
    }
    childNode = stack.pop()
  }

  confirmDelete(confirm, {
    header: l.value.manageCategories.confirmCategoryDeleteHeader,
    message: ls.l('manageCategories.confirmCategoryDeleteText', node.label ?? '', total ?? ''),
    accept: () => deleteCategory(node)
  })
}

const deleteCategory = async (node: TreeNode) => {
  loadingCategories.value = true
  await categoryService.deleteCategory([parseInt(node.key)])
  loadCategories()
}
</script>

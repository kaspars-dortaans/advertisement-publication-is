<template>
  <Tree
    ref="tree"
    v-model:selectionKeys="selectedCategoryKeys"
    v-model:expanded-keys="expandedCategoryKeys"
    :value="displayedCategoryNodes"
    selectionMode="single"
    @nodeSelect="handleCategorySelection"
    @node-expand="collapseOtherNodes"
    @node-collapse="displayRootNodes"
  ></Tree>
</template>

<script setup lang="ts">
import { CategoryClient, CategoryItem } from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import { buildNodeHierarchy } from '@/utils/build-node-hierarchy'
import { getClient } from '@/utils/client-builder'
import type { TreeNode } from 'primevue/treenode'
import { nextTick, onMounted, ref, watch } from 'vue'

// Component output
const model = defineModel()
const selectedCategoryNameModel = defineModel('selectedCategoryName')

// Services
const categoryService = getClient(CategoryClient)
const l = LocaleService.currentLocale

// Reactive data
const categoryNodes = ref<TreeNode[]>([])
const displayedCategoryNodes = ref<TreeNode[]>([])
const selectedCategoryKeys = ref({})
const expandedCategoryKeys = ref<{ [key: string]: boolean }>({})

// Constants
const newCategoryKey = 'new-category'

// Hooks
onMounted(() => {
  loadCategories()
})

const loadCategories = async () => {
  //Add first 'new category' - show all advertisements
  categoryNodes.value = [
    {
      key: newCategoryKey,
      label: l.value.categoryMenu.new,
      children: [],
      leaf: true,
      data: {
        parent: null
      }
    } as TreeNode
  ]
  const categories = await categoryService.getCategories()
  categoryNodes.value = [
    ...categoryNodes.value,
    ...buildNodeHierarchy<CategoryItem, TreeNode>(
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
  ]

  //Update selected category name v-model
  const selectedKeys = Object.keys(selectedCategoryKeys.value)
  if (selectedKeys.length) {
    selectedCategoryNameModel.value = categoryNodes.value.find(
      (n) => n.key === selectedKeys[0]
    )?.label
  }
  displayedCategoryNodes.value = categoryNodes.value
}

// Watchers
watch(
  model,
  (newValue) => {
    let idStr: number | string | undefined
    if (typeof newValue === 'number') {
      idStr = '' + newValue
    } else if (newValue === null) {
      idStr = newCategoryKey
    } else if (newValue === undefined || typeof newValue === 'string') {
      idStr = newValue
    } else {
      return
    }

    const selectedKeys = Object.keys(selectedCategoryKeys.value)
    const selectedIdStr = selectedKeys.length ? selectedKeys[0] : undefined
    if (idStr !== selectedIdStr) {
      selectedCategoryKeys.value = idStr !== undefined ? { [idStr]: true } : {}

      //Update selected category name v-model
      selectedCategoryNameModel.value = categoryNodes.value.find((n) => n.key == idStr)?.label
    }
  },
  { immediate: true }
)

watch(LocaleService.currentLocaleName, async () => {
  await loadCategories()
})

// Methods
const handleCategorySelection = (categoryNode: TreeNode) => {
  const id =
    categoryNode.key === newCategoryKey
      ? null
      : categoryNode.key
        ? parseInt(categoryNode.key)
        : categoryNode.key
  model.value = id

  //Update selected category name v-model
  selectedCategoryNameModel.value = categoryNode.label

  // Display selected node and its child nodes
  if (categoryNode.leaf === false) {
    displayedCategoryNodes.value = [categoryNode]
    collapseOtherNodes(categoryNode)
  } else if (categoryNode.data.parent) {
    displayedCategoryNodes.value = [categoryNode.data.parent]
    collapseOtherNodes(categoryNode.data.parent)
  }
}

const getNodeDirectParentIds = (node: TreeNode) => {
  const parentIds = []
  let currentNode = node
  while (currentNode.data?.parent) {
    parentIds.push(currentNode.data.parent.key)
    currentNode = currentNode.data.parent
  }
  return parentIds
}

const getExpandedChildIds = (categoryNode: TreeNode) => {
  if (!categoryNode.children) {
    return []
  }

  const expandedChildNodeIds = []
  const searchableNodes = [...categoryNode.children]
  let currentChildNode = searchableNodes.pop()
  while (currentChildNode) {
    if (
      currentChildNode.key in expandedCategoryKeys.value &&
      expandedCategoryKeys.value[currentChildNode.key]
    ) {
      expandedChildNodeIds.push(currentChildNode.key)
      if (currentChildNode.children) {
        searchableNodes.push(...currentChildNode.children)
      }
    }
    currentChildNode = searchableNodes.pop()
  }
  return expandedChildNodeIds
}

const collapseOtherNodes = (categoryNode: TreeNode) => {
  const parentIds = getNodeDirectParentIds(categoryNode)
  const childIds = getExpandedChildIds(categoryNode)
  const expandedIds = parentIds.concat(childIds)
  const expandedKeys: { [key: string]: boolean } = { [categoryNode.key]: true }

  for (const id of expandedIds) {
    expandedKeys[id] = true
  }
  nextTick(() => (expandedCategoryKeys.value = expandedKeys))
}

const displayRootNodes = (categoryNode: TreeNode) => {
  // If first displayed node was collapsed and not all root nodes are collapsed
  // then display expanded parent node if it exists otherwise display all root nodes
  if (
    categoryNode.key === displayedCategoryNodes.value[0].key &&
    categoryNodes.value !== displayedCategoryNodes.value
  ) {
    displayedCategoryNodes.value = categoryNode.data.parent
      ? [categoryNode.data.parent]
      : categoryNodes.value

    if (displayedCategoryNodes.value.length === 1 && !displayedCategoryNodes.value[0].leaf) {
      expandedCategoryKeys.value = { [displayedCategoryNodes.value[0].key]: true }
    }
  }
}
</script>

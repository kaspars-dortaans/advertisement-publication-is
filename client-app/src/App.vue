<template>
  <MenuBar :model="items">
    <template #start>
      <h3>Site title</h3>
    </template>
    <template #item="{ item, props, hasSubmenu }">
      <router-link v-if="item.route" v-slot="{ href, navigate }" :to="item.route" custom>
        <a :href v-bind="props.action" @click="navigate">
          <span :class="item.icon"></span>
          <span class="ml-2">{{ item.label }}</span>
        </a>
      </router-link>
      <a v-else :href="item.url" :target="item.target" v-bind="props.action">
        <span :class="item.icon"></span>
        <span class="ml-2">{{ item.label }}</span>
        <span v-if="hasSubmenu" class="pi pi-fw pi-angle-down ml-2"></span>
      </a>
    </template>
    <template #end>
      <span>User profile icon</span>
    </template>
  </MenuBar>

  <RouterView />
</template>

<script setup lang="ts">
import { RouterLink, RouterView } from 'vue-router'
import { ref, type Ref } from 'vue'
import type { MenuItem } from 'primevue/menuitem'

const items: Ref<MenuItem[]> = ref([
  {
    label: 'Home',
    icon: 'pi pi-home'
  },
  {
    label: 'Test',
    items: [
      {
        label: 'sub-item1'
      },
      {
        label: 'sub-item2'
      }
    ]
  }
])
</script>

<style scoped></style>

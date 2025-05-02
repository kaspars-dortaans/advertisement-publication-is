<template>
  <NavBar></NavBar>
  <RouterView />

  <ConfirmDialog />
  <Toast position="top-right" group="tr" />
</template>

<script setup lang="ts">
import NavBar from '@/components/NavBar.vue'
import { ConfirmDialog } from 'primevue'
import Toast from 'primevue/toast'
</script>

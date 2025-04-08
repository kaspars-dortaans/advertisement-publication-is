<template>
  <NavBar></NavBar>

  <RouterView />
  <ConfirmDialog />
</template>

<script setup lang="ts">
import NavBar from '@/components/NavBar.vue'
import { ConfirmDialog } from 'primevue'
</script>

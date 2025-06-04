<template>
  <ResponsiveLayout>
    <div class="flex-1 flex flex-col">
      <Panel class="rounded-none lg:rounded-md flex-1">
        <template #header>
          <div class="panel-title-container">
            <BackButton :defaultTo="{ name: 'manageCategories' }" />
            <h4 class="page-title">
              {{ l.navigation.viewTermsOfUse }}
            </h4>
          </div>
        </template>

        <p>{{ l.termsOfUse }}</p>
      </Panel>
    </div>
  </ResponsiveLayout>
</template>

<script lang="ts" setup>
import BackButton from '@/components/common/BackButton.vue'
import ResponsiveLayout from '@/components/common/ResponsiveLayout.vue'
import { LocaleService } from '@/services/locale-service'

//Services
const l = LocaleService.currentLocale
</script>

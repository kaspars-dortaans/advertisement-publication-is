<template>
  <div class="flex flex-col gap-0.5">
    <p v-if="paymentInfo?.date" class="inline-flex">
      <span class="flex-1 font-semibold">{{ l.paymentDetails.date }}</span>
      <span class="flex-1">{{ dateFormat.format(paymentInfo.date) }}</span>
    </p>
    <p v-if="paymentInfo?.payerUsername" class="inline-flex">
      <span class="flex-1 font-semibold">{{ l.paymentDetails.payerUsername }}</span>
      <span class="flex-1">{{ paymentInfo.payerUsername }}</span>
    </p>
    <p class="font-semibold">{{ l.paymentDetails.paymentItems }}</p>
    <div class="lg:overflow-y-auto flex flex-col gap-2">
      <div
        v-for="paymentItem in paymentInfo?.items"
        :key="'' + paymentItem.type + paymentItem.paymentSubjectId"
        class="p-2 border border-black rounded-md"
      >
        <h5 class="font-semibold">{{ l.paymentType[paymentItem.type!] }}</h5>
        <dl class="grid grid-cols-2 gap-x-3">
          <dt>{{ l.paymentDetails.title }}</dt>
          <dd>{{ paymentItem.title }}</dd>

          <dt>{{ l.paymentDetails.timePeriod }}</dt>
          <dd>{{ getPostTimeTitle(ls, paymentItem.timePeriod) }}</dd>

          <dt>{{ l.paymentDetails.price }}</dt>
          <dd>{{ currencyFormat.format(paymentItem.price!) }}</dd>
        </dl>
      </div>
    </div>
    <h4 class="font-semibold text-lg">
      {{ ls.l('paymentDetails.totalAmount', currencyFormat.format(paymentInfo?.totalAmount ?? 0)) }}
    </h4>
  </div>
</template>

<script lang="ts" setup>
import { getPostTimeTitle } from '@/constants/advertisement-post-time-span'
import type { PriceInfo } from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import { computed } from 'vue'

defineProps<{
  paymentInfo: PriceInfo | undefined
}>()

const l = LocaleService.currentLocale
const ls = LocaleService.get()

const currencyFormat = computed(() =>
  Intl.NumberFormat(LocaleService.currentLocaleName.value, {
    style: 'currency',
    currency: 'EUR'
  })
)
const dateFormat = computed(() =>
  Intl.DateTimeFormat(LocaleService.currentLocaleName.value, {
    dateStyle: 'short',
    timeStyle: 'short'
  })
)
</script>

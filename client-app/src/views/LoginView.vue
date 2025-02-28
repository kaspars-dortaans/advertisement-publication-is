<template>
  <div class="flex items-center justify-center flex-1 bg-primary">
    <Panel class="m-2">
      <template #header>
        <span class="text-2xl">{{ l.navigation.login }}</span>
      </template>
      <form class="flex-none flex flex-col items-center gap-2 min-w-80 bg-white" @submit="tryLogin">
        <Message v-if="fieldHelper.formErrors.value" severity="error">{{
          fieldHelper.formErrors
        }}</Message>

        <InputGroup>
          <InputGroupAddon>
            <i class="pi pi-at"></i>
          </InputGroupAddon>
          <InputText
            v-model="fields.email!.value"
            v-bind="fields.email?.attributes"
            :placeholder="l.form.email"
            :invalid="fields.email?.hasError"
          />
        </InputGroup>
        <FieldError :field="fields.email" />

        <InputGroup>
          <InputGroupAddon>
            <i class="pi pi-key"></i>
          </InputGroupAddon>
          <Password
            v-model="fields.password!.value"
            v-bind="fields.password?.attributes"
            :placeholder="l.form.password"
            :feedback="false"
            :invalid="fields.email?.hasError"
          />
        </InputGroup>
        <FieldError :field="fields.password" />

        <Button class="mt-1" :label="l.navigation.login" @click="tryLogin" />
        <p>
          <span>{{ l.form.doNotHaveAnAccountQuestion }}</span>
          <RouterLink class="ml-1 link" :to="{ name: 'register' }">{{
            l.navigation.register
          }}</RouterLink>
        </p>
      </form>
    </Panel>
  </div>
</template>

<script setup lang="ts">
import FieldError from '@/components/Form/FieldError.vue'
import { LoginDto, type ILoginDto } from '@/services/api-client'
import { AuthService } from '@/services/auth-service'
import { LocaleService } from '@/services/locale-service'
import { FieldHelper } from '@/utils/field-helper'
import { toTypedSchema } from '@vee-validate/yup'
import { useForm } from 'vee-validate'
import { useRouter } from 'vue-router'
import { object, string } from 'yup'

//Props
const props = defineProps<{ redirect: boolean }>()

//Route
const router = useRouter()

//Services
const authService = new AuthService()
const l = LocaleService.currentLocale

//Form
const form = useForm({
  validationSchema: toTypedSchema(
    object({
      email: string().required().email(),
      password: string().required()
    })
  )
})
const { values, handleSubmit } = form
const fieldHelper = new FieldHelper<ILoginDto>(form)
fieldHelper.defineMultipleFields(['email', 'password'])
const fields = fieldHelper.fields

const tryLogin = handleSubmit(async () => {
  const loginDto = new LoginDto(values)
  try {
    await authService.login(loginDto)
    if (props.redirect) {
      router.back()
    } else {
      router.push({ name: 'home' })
    }
  } catch (error) {
    fieldHelper.handleErrors(error)
  }
})
</script>

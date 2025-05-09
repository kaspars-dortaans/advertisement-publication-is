<template>
  <ResponsiveLayout>
    <Panel class="my-auto mx-2">
      <template #header>
        <span class="text-2xl">{{ l.navigation.login }}</span>
      </template>
      <form class="flex-none flex flex-col items-center gap-2 min-w-80 bg-white" @submit="tryLogin">
        <Message v-if="formErrors" severity="error">{{ formErrors }}</Message>

        <InputGroup>
          <InputGroupAddon>
            <i class="pi pi-at"></i>
          </InputGroupAddon>
          <InputText
            v-model="fields.email!.value"
            v-bind="fields.email?.attributes"
            :placeholder="l.form.login.email"
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
            :placeholder="l.form.login.password"
            :feedback="false"
            :invalid="fields.email?.hasError"
          />
        </InputGroup>
        <FieldError :field="fields.password" />

        <Button class="mt-1" :label="l.navigation.login" :loading="isSubmitting" type="submit" />
        <p>
          <span>{{ l.form.login.doNotHaveAnAccountQuestion }}</span>
          <RouterLink class="ml-1 link" :to="{ name: 'register' }">{{
            l.navigation.register
          }}</RouterLink>
        </p>
      </form>
    </Panel>
  </ResponsiveLayout>
</template>

<script setup lang="ts">
import ResponsiveLayout from '@/components/common/ResponsiveLayout.vue'
import FieldError from '@/components/form/FieldError.vue'
import { LoginDto, type ILoginDto } from '@/services/api-client'
import { AppNavigation } from '@/services/app-navigation'
import { AuthService } from '@/services/auth-service'
import { LocaleService } from '@/services/locale-service'
import { FieldHelper } from '@/utils/field-helper'
import { toTypedSchema } from '@vee-validate/yup'
import { useForm } from 'vee-validate'
import { watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { object, string } from 'yup'

//Props
const props = defineProps<{ redirect: boolean }>()

//Route
const { push } = useRouter()
const { redirectedFrom } = useRoute()

//Services
const authService = AuthService.get()
const l = LocaleService.currentLocale
const navigation = AppNavigation.get()

//Form
const form = useForm<ILoginDto>({
  validationSchema: toTypedSchema(
    object({
      email: string().required().email().label('form.login.email'),
      password: string().required().label('form.login.password')
    })
  )
})
const { values, handleSubmit, isSubmitting, validate } = form
const { fields, formErrors, defineMultipleFields, handleErrors } = new FieldHelper(form)
defineMultipleFields(['email', 'password'])

const tryLogin = handleSubmit(async () => {
  const loginDto = new LoginDto(values)
  try {
    await authService.login(loginDto)

    if (props.redirect && redirectedFrom) {
      //If redirected to login when trying access route which requires permissions while unauthorized
      push(redirectedFrom.fullPath)
    } else if (props.redirect && navigation.hasPrevious()) {
      //If redirected to login when api request returned unauthorized code
      push(navigation.getPreviousFullPath)
    } else {
      //Fall back to home page
      push({ name: 'home' })
    }
  } catch (error) {
    handleErrors(error)
  }
})

//Watchers
watch(LocaleService.currentLocaleName, () => {
  validate({ mode: 'validated-only' })
})
</script>

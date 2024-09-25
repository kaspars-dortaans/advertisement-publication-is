<template>
  <div class="flex items-center justify-center flex-1 bg-primary">
    <Panel>
      <template #header>
        <span class="text-2xl">{{ ls.l('register') }}</span>
      </template>
      <form class="flex flex-col gap-3 items-center bg-white" @submit="onSubmit">
        <div class="flex gap-5">
          <div class="flex flex-col gap-2 min-w-80">
            <Message v-if="fieldHelper.hasFormErrors.value" severity="error">{{ fieldHelper.formErrors }}</Message>

            <InputText v-model="fields.firstName!.value" :placeholder="ls.l('firstName')"
              :invalid="fields.firstName!.hasError" />
            <FieldError :field="fields.firstName" />

            <InputText v-model="fields.lastName!.value" :placeholder="ls.l('lastName')"
              :invalid="fields.lastName!.hasError" />
            <FieldError :field="fields.lastName" />

            <InputText v-model="fields.userName!.value" :placeholder="ls.l('username')"
              :invalid="fields.userName!.hasError" />
            <FieldError :field="fields.userName" />

            <InputText v-model="fields.email!.value" v-bind="fields.email!.attributes" :placeholder="ls.l('email')"
              :invalid="fields.email!.hasError" />
            <FieldError :field="fields.email" />

            <div class="flex items-center">
              <Checkbox v-model="fields.isEmailPublic!.value" :invalid="fields.isEmailPublic!.hasError" :binary="true"
                inputId="register.isEmailPublic" />
              <label class="ml-2" for="register.isEmailPublic">{{ ls.l('publiclyDisplayEmail') }}</label>
            </div>
            <FieldError :field="fields.isEmailPublic" />

            <InputText v-model="fields.phoneNumber!.value" :placeholder="ls.l('phoneNumber')"
              :invalid="fields.phoneNumber!.hasError" />
            <FieldError :field="fields.phoneNumber" />

            <div class="flex items-center">
              <Checkbox v-model="fields.isPhoneNumberPublic!.value" :invalid="!!fields.isPhoneNumberPublic!.hasError"
                :binary="true" inputId="register.isPhonePublic" />
              <label class="ml-2" for="register.isPhonePublic">{{ ls.l('publiclyDisplayPhoneNumber') }}</label>
            </div>
            <FieldError :field="fields.isPhoneNumberPublic" />

            <Password v-model="fields.password!.value" :placeholder="ls.l('password')"
              :invalid="fields.password!.hasError" />
            <FieldError :field="fields.password" />

            <Password v-model="fields.passwordConfirmation!.value" :placeholder="ls.l('confirmPassword')"
              :feedback="false" :invalid="fields.passwordConfirmation!.hasError" />
            <FieldError :field="fields.passwordConfirmation" />
          </div>

          <div>
            TODO: Profile picture input
          </div>
        </div>

        <Button type="submit" :label="ls.l('register')" />
        <p>
          <span>{{ ls.l('alreadyHaveAnAccount') }}</span>
          <RouterLink class="ml-1 link" :to="{ name: 'login' }">{{ ls.l('login') }}</RouterLink>
        </p>
      </form>
    </Panel>
  </div>
</template>

<script setup lang="ts">
import FieldError from '@/components/FieldError.vue';
import { RegisterDto, type IRegisterDto } from '@/services/api-client';
import { LocaleService } from '@/services/locale-service';
import { getCLient } from '@/utils/client-builder';
import { FieldHelper } from '@/utils/field-helper';
import { useForm } from 'vee-validate';
import { useRouter } from 'vue-router';

const api = getCLient()
const ls = new LocaleService()
const router = useRouter()

const form = useForm<IRegisterDto>()
const fieldHelper = new FieldHelper<IRegisterDto>(form)
const { values, handleSubmit, validate } = form

fieldHelper.defineMultipleFields([
  "firstName",
  "lastName",
  "userName",
  "email",
  "isEmailPublic",
  "phoneNumber",
  "isPhoneNumberPublic",
  "password",
  "passwordConfirmation"
])
const fields = fieldHelper.fields

const onSubmit = handleSubmit(async () => {
  fieldHelper.clearErrors()
  await validate()
  const registerDto = new RegisterDto(values)
  try {
    await api.register(registerDto)
    router.push({ name: 'login' })
  } catch (error) {
    fieldHelper.handleErrors(error)
  }
})
</script>

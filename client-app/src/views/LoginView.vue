<template>
  <div class="flex items-center justify-center flex-1 bg-primary">
    <Panel>
      <template #header>
        <span class="text-2xl">{{ ls.l('login') }}</span>
      </template>
      <form class="flex-none flex flex-col items-center gap-2 min-w-80 bg-white">
        <Message v-if="formError" severity="error">{{ ls.l(formError) }}</Message>
        <InputGroup>
          <InputGroupAddon>
            <i class="pi pi-at"></i>
          </InputGroupAddon>
          <InputText v-model="email" :placeholder="ls.l('email')" :invalid="!!formError" />
        </InputGroup>

        <InputGroup>
          <InputGroupAddon>
            <i class="pi pi-key"></i>
          </InputGroupAddon>
          <Password v-model="password" :placeholder="ls.l('password')" :feedback="false" :invalid="!!formError" />
        </InputGroup>

        <Button class="mt-1" :label="ls.l('login')" @click="tryLogin" />
        <p>
          <span>{{ ls.l('dontHaveAnAccountQuestion') }}</span>
          <RouterLink class="ml-1 link" :to="{ name: 'home' }">{{ ls.l('register') }}</RouterLink>
        </p>
      </form>
    </Panel>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { AuthService } from '@/services/auth-service';
import { ApiException, LoginDto } from '@/services/api-client';
import { LocaleService } from '@/services/locale-service';
import { useRouter } from 'vue-router';

const authService = new AuthService()
const ls = new LocaleService()
const router = useRouter()

const email = ref('')
const password = ref('')
const formError = ref('')

async function tryLogin() {
  const loginDto = new LoginDto({
    email: email.value,
    password: password.value
  })
  try {
    await authService.login(loginDto)
    router.push({ name: 'home' })
  } catch (error) {
    if (ApiException.isApiException(error)) {
      formError.value = error?.response ?? error?.message
    } else {
      throw error
    }
  }
}
</script>

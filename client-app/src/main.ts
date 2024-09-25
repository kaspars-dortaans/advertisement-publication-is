import './assets/style.css'
import 'primeicons/primeicons.css'

import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import { initPrimeVue } from './init/primevue'
import initAxios from './init/axios'
import { initYup } from './init/yup'

const app = createApp(App)

app.use(router)

initPrimeVue(app)
initAxios(app)
initYup(app)

app.mount('#app')

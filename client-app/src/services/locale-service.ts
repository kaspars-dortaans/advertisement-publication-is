import axios from "axios";
import { usePrimeVue } from "primevue/config";

export class LocaleService {
    _primevue: ReturnType<typeof usePrimeVue> | undefined

    constructor(primevue?: ReturnType<typeof usePrimeVue>){
        this._primevue = primevue
    }

    get primevue() {
        if(!this._primevue)
            this._primevue = usePrimeVue()

        return this._primevue
    }

    async loadLocale(name: string){
        const locale = await this.fetchLocale(name)
        this.primevue.config.locale = locale.data
    }

    async fetchLocale(name: string){
        const localePath = `src/locales/${name}.json`
        return await axios.get(localePath, { baseURL: import.meta.env.BASE_URL })
    }

    l(key: string){
        const locale = this.primevue.config.locale as object
        if(key in locale)
            return locale[key as keyof object]

        return key
    }
}
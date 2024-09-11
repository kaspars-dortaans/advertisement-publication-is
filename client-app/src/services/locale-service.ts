import { axiosInstance } from "@/init/axios";
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
        return await axiosInstance.get(localePath, { baseURL: import.meta.env.BASE_URL })
    }

    l(keyString: string){
        const locale = this.primevue.config.locale as object
        const keys = keyString.split('.')
        let object = locale

        for(const key of keys){
            if(key in object)
                object = object[key as keyof object]
            else 
                return keyString
        }
        return "" + object
    }
}
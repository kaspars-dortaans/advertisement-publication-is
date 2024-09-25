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

    localizeMultiple(keys: string[], suffix?: string, ...params: string[]){
        return keys.map(k => this.l((suffix ?? "") + k, ...params))
    }

    l(keyString: string, ...params: (string | number)[]){
        const locale = this.primevue.config.locale as object
        const keys = keyString.split('.')
        let object = locale

        for(const key of keys){
            if(key in object){
                object = object[key as keyof object]
            } else 
                return keyString
        }
        return this.insertParamsIntoString(object + "", params)
    }

    insertParamsIntoString(str: string, params: (string | number)[]){
        if(!params.length){
            return str;
        }

        const paramsPrefix = "{";
        const paramSuffix = "}";
        let result = str
        let lastParamIndex = 0, prefixIndex = -1, suffixIndex = -1;
        for(const param of params){
            console.log("param ", param)
            prefixIndex = result.indexOf(paramsPrefix, lastParamIndex);
            suffixIndex = result.indexOf(paramSuffix, prefixIndex);
            if(prefixIndex >= 0 && suffixIndex >= 0){
                result = result.slice(0, prefixIndex) + param + result.slice(suffixIndex+1)
                lastParamIndex = prefixIndex
            } else {
                break;
            }
        }
        return result
    }
}
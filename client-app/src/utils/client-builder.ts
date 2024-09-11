import { axiosInstance } from "@/init/axios"
import { Client } from "@/services/api-client"

export const getCLient = () => {
    return new Client("", axiosInstance)
}
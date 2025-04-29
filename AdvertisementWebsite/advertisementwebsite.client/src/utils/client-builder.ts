import { axiosInstance } from '@/init/axios'
import type { AxiosInstance } from 'axios'

export function getClient<ClientClass> (Client: new (baseUrl?: string, axiosInstance?: AxiosInstance) => ClientClass) {
  return new Client('', axiosInstance)
}

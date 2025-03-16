import { axiosInstance } from '@/init/axios'
import { HttpStatusCode } from 'axios'

export const downloadFile = async (url: string) => {
  const response = await axiosInstance.get(url, {
    responseType: 'blob'
  })

  if (response.status === HttpStatusCode.Ok) {
    const contentDisposition = response.headers['content-disposition'] as string
    const fileName = contentDisposition.match(/filename\s*=(.+);/)?.[1] ?? ''
    return new File([response.data], fileName)
  }
}

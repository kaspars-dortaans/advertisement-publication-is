import axios from "axios";

export const initAxios = () => {
    console.log("api url", import.meta.env.VITE_API_URL)
    axios.defaults.baseURL = import.meta.env.VITE_API_URL
}

export default initAxios
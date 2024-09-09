import axios from "axios";

export const initAxios = () => {
    axios.defaults.baseURL = import.meta.env.VITE_API_URL
}

export default initAxios
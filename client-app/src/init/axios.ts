import axios from "axios";

export const initAxios = () => {
    axios.defaults.baseURL = 'https://localhost:7076/'
}

export default initAxios
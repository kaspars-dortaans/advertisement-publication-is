import { fileURLToPath, URL } from 'node:url';

import vue from '@vitejs/plugin-vue';
import { ConfigEnv, loadEnv, UserConfig } from 'vite';

// https://vitejs.dev/config/
export default ((configEnv: ConfigEnv) => {
  const env = loadEnv(configEnv.mode, process.cwd());

  return {
    plugins: [
      vue(),
    ],
    base: env.VITE_BASE_URL,
    resolve: {
      alias: {
        '@': fileURLToPath(new URL('./src', import.meta.url))
      }
    }
  } as UserConfig
})

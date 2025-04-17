/**
 * Based on https://github.com/johndatserakis/vue-screen-size
 */

import { computed, onMounted, onUnmounted, ref } from 'vue'

/**
 * Composable function to keep track of screen size
 * @returns computed refs of screen size
 */
export function useTrackScreenSize() {
  const getScreenWidth = () => {
    return window.innerWidth || document.documentElement.clientWidth || document.body.clientWidth
  }
  const getScreenHeight = () => {
    return window.innerHeight || document.documentElement.clientHeight || document.body.clientHeight
  }

  const widthRef = ref<number | null>(null)
  const heightRef = ref<number | null>(null)

  const handleResize = () => {
    widthRef.value = getScreenWidth()
    heightRef.value = getScreenHeight()
  }

  const width = computed(() => {
    if (!widthRef.value) {
      widthRef.value = getScreenWidth()
    }
    return widthRef.value
  })
  const height = computed(() => {
    if (!heightRef.value) {
      heightRef.value = getScreenHeight()
    }
    return heightRef.value
  })

  onMounted(() => {
    window.addEventListener('resize', handleResize)
  })
  onUnmounted(() => {
    window.removeEventListener('resize', handleResize)
  })

  return { width, height }
}

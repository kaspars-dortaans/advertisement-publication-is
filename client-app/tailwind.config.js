/** @type {import('tailwindcss').Config} */
export default {
  darkMode: 'class', // This enables dark mode based on the presence of the "dark" class in the HTML tag
  content: ['./index.html', './src/**/*.{vue,js,ts,jsx,tsx}', './src/*/.{vue,js,ts,jsx,tsx}'],
  plugins: [require('tailwindcss-primeui')]
}

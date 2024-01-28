<template>
    <BForm @submit="onSubmit" @reset="onReset" v-if="show">
      <BFormGroup
        id="input-group-1"
        label="Email address:"
        label-for="input-1"
        description="We'll never share your email with anyone else."
      >
        <BFormInput
          id="input-1"
          v-model="email"
          type="email"
          placeholder="Enter email"
          required
        />
      </BFormGroup>
  
      <BFormGroup
        id="password-input-group"
        label="Password:"
        label-for="password-input"
        description="Please enter your password"
      >
        <BFormInput
            id="password-input"
            v-model="password"
            type="password"
            required
        />
      </BFormGroup>
      
      <BButton type="submit" variant="primary">Submit</BButton>
      <BButton type="reset" variant="danger">Reset</BButton>
    </BForm>
  </template>
  
  <script lang="ts">
import { Component, Vue, toNative } from 'vue-facing-decorator'

    @Component
    class LoginView extends Vue {

        show = true;
        email = "";
        password = "";

        onSubmit(){
            console.log("submit", this.email, this.password);
        }

        onReset(){
            console.log("reset");
        }
    }

    export default toNative(LoginView)
  </script>
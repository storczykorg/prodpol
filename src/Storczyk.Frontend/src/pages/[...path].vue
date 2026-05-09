<script lang="ts" setup>
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
</script>

<template>
  <article class="prose">
  <h1> {{ t( "message.resource_not_found" ) }} </h1><br/>
  <h2> {{ t( "action.take_me" ) }}
    <router-link to="/">
    {{ t( "site.homepage" ) }}
    </router-link>
  </h2>
  </article>
</template>

<style scoped>

</style>
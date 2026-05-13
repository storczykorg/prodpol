<script setup lang="ts">

import commitInfo from "virtual:commitInfo";
import {useServerPing} from "../../data/server/status.ts";

const { data, isLoading, error } = useServerPing();

</script>

<template>
<article class="container flex flex-col gap-4">
  <div>

  </div>
  <div class="card bg-base-100 w-80 shadow-sm">
    <div class="card-body">
    <h2 class="card-title">Status serwera</h2>
    <div class="p-4">
      <template v-if="isLoading">
        <div class="inline-grid *:[grid-area:1/1]">
          <div class="status status-lg status-error animate-ping"></div>
          <div class="status status-lg status-error"></div>
        </div>
        Checking status
      </template>
      <template v-else-if="error">
        <div class="inline-grid *:[grid-area:1/1]">
          <div class="status status-lg status-error animate-ping"></div>
          <div class="status status-lg status-error"></div>
        </div>
        Server is down
      </template>
      <template v-else-if="data">
        <div class="inline-grid *:[grid-area:1/1]">
          <div class="status status-lg animate-ping"></div>
          <div class="status status-lg"></div>
        </div>
        Server is up
      </template>
    </div>
    </div>
  </div>
  <div class="stats shadow">
      <div class="stat">
        <div class="stat-figure text-primary">
          <svg
              xmlns="http://www.w3.org/2000/svg"
              fill="none"
              viewBox="0 0 24 24"
              class="inline-block h-8 w-8 stroke-current"
          >
            <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z"
            ></path>
          </svg>
        </div>
        <div class="stat-title">Total Likes</div>
        <div class="stat-value text-primary">25.6K</div>
        <div class="stat-desc">21% more than last month</div>
      </div>

      <div class="stat">
        <div class="stat-figure text-secondary">
          <svg
              xmlns="http://www.w3.org/2000/svg"
              fill="none"
              viewBox="0 0 24 24"
              class="inline-block h-8 w-8 stroke-current"
          >
            <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M13 10V3L4 14h7v7l9-11h-7z"
            ></path>
          </svg>
        </div>
        <div class="stat-title">Page Views</div>
        <div class="stat-value text-secondary">2.6M</div>
        <div class="stat-desc">21% more than last month</div>
      </div>

      <div class="stat">
        <div class="stat-figure text-secondary">
          <div class="avatar avatar-online">
            <div class="w-16 rounded-full">
              <img src="https://img.daisyui.com/images/profile/demo/anakeen@192.webp" />
            </div>
          </div>
        </div>
        <div class="stat-value">86%</div>
        <div class="stat-title">Tasks done</div>
        <div class="stat-desc text-secondary">31 tasks remaining</div>
      </div>
    </div>
    <div class="stats shadow">
      <div class="stat">
        <div class="stat-figure text-secondary">
          <svg
              xmlns="http://www.w3.org/2000/svg"
              fill="none"
              viewBox="0 0 24 24"
              class="inline-block h-8 w-8 stroke-current"
          >
            <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
            ></path>
          </svg>
        </div>
        <div class="stat-title">Downloads</div>
        <div class="stat-value">31K</div>
        <div class="stat-desc">Jan 1st - Feb 1st</div>
      </div>

      <div class="stat">
        <div class="stat-figure text-secondary">
          <svg
              xmlns="http://www.w3.org/2000/svg"
              fill="none"
              viewBox="0 0 24 24"
              class="inline-block h-8 w-8 stroke-current"
          >
            <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M12 6V4m0 2a2 2 0 100 4m0-4a2 2 0 110 4m-6 8a2 2 0 100-4m0 4a2 2 0 110-4m0 4v2m0-6V4m6 6v10m6-2a2 2 0 100-4m0 4a2 2 0 110-4m0 4v2m0-6V4"
            ></path>
          </svg>
        </div>
        <div class="stat-title">New Users</div>
        <div class="stat-value">4,200</div>
        <div class="stat-desc">↗︎ 400 (22%)</div>
      </div>

      <div class="stat">
        <div class="stat-figure text-secondary">
          <svg
              xmlns="http://www.w3.org/2000/svg"
              fill="none"
              viewBox="0 0 24 24"
              class="inline-block h-8 w-8 stroke-current"
          >
            <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M5 8h14M5 8a2 2 0 110-4h14a2 2 0 110 4M5 8v10a2 2 0 002 2h10a2 2 0 002-2V8m-9 4h4"
            ></path>
          </svg>
        </div>
        <div class="stat-title">New Registers</div>
        <div class="stat-value">1,200</div>
        <div class="stat-desc">↘︎ 90 (14%)</div>
      </div>
    </div>
  <div>
    <div class="p-4 rounded-md shadow w-full">
      <h2 class="font-semibold">Wersja frontendu</h2>
      <div class="p-4">
        <code class="whitespace-pre text-sm">
          {{ String(commitInfo) }}
        </code>
      </div>
    </div>
  </div>
</article>
</template>

<style scoped>

</style>
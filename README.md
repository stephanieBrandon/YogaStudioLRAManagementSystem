# Yoga Studio LRA Management System — Deployment Fork

This is a fork of [n01570640/YogaStudioLRAManagementSystem](https://github.com/n01570640/YogaStudioLRAManagementSystem) configured for cloud deployment on Render.

## Live Demo

**Deployed URL:** https://yogastudiolramanagementsystem-main.onrender.com

> Free-tier services sleep after 15 minutes of inactivity. First request after idle takes ~30 seconds to wake up.

## How this fork differs from the original

- Migrated from Oracle → PostgreSQL
- Added Dockerfile for container deployment
- Configured forwarded headers middleware for Render's reverse proxy
- Deployed API URL hardcoded in ClockIn view (replaces localhost)
- DateHelper converts timestamps to Eastern Time for display

## For the original project setup

See the [original repository](https://github.com/n01570640/YogaStudioLRAManagementSystem) — this fork is not intended for local development.

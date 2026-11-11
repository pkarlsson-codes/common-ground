// Runs Playwright and, when the run fails, copies its console output, HTML report and traces to
// failed-runs/<time>/ so the next run can't overwrite them. Keeps the newest KEEP failed runs.
// Arguments pass through to `playwright test`.
import { spawn } from 'node:child_process'
import { cpSync, existsSync, mkdirSync, readdirSync, rmSync, writeFileSync } from 'node:fs'
import { join } from 'node:path'

const KEEP = 5
const here = new URL('.', import.meta.url).pathname
const failedRuns = join(here, 'failed-runs')

const output = []
const child = spawn('npx', ['playwright', 'test', ...process.argv.slice(2)], { cwd: here, stdio: ['inherit', 'pipe', 'pipe'] })
child.stdout.on('data', chunk => { process.stdout.write(chunk); output.push(chunk) })
child.stderr.on('data', chunk => { process.stderr.write(chunk); output.push(chunk) })

child.on('close', code => {
  if (code !== 0) {
    const dir = join(failedRuns, new Date().toISOString().replace(/[:.]/g, '-'))
    mkdirSync(dir, { recursive: true })
    writeFileSync(join(dir, 'output.txt'), Buffer.concat(output))
    for (const folder of ['playwright-report', 'test-results']) {
      if (existsSync(join(here, folder))) cpSync(join(here, folder), join(dir, folder), { recursive: true })
    }
    for (const old of readdirSync(failedRuns).sort().slice(0, -KEEP)) {
      rmSync(join(failedRuns, old), { recursive: true, force: true })
    }
    console.error(`\nFailed run saved to ${dir} (output.txt; open the report with: npx playwright show-report ${join(dir, 'playwright-report')})`)
  }
  process.exit(code ?? 1)
})

import subprocess

# Checkout the main branch.
subprocess.check_call(['git', 'checkout', 'main'])

# Get the branch name from the user.
branch_name = input('Enter the name of the new branch, following the convention year/pi#: ')

# Checkout a new branch based on main.
subprocess.check_call(['git', 'checkout', '-b', branch_name])

# Generate the subset of commits that belong to v.next but not main.
commits = subprocess.check_output(['git', 'log', '--pretty=format:%H', 'v.next', '^main']).decode('utf-8').split('\n')

# Iterate through the commit subset in chronological order.
ignore_case = "[Automated] Sync v.next with main"
failed_commits = []
print("Cherry-picking commits:\n")
for commit in reversed(commits):

    # Store the commit message.
    message = subprocess.check_output(['git', 'log', '-1', '--pretty=format:%s', commit]).decode('utf-8')
    
    # If the commit message contains the ignore case string, skip it.
    if ignore_case in message:
        continue

    # Cherry-pick the commit to the new branch.
    try:
        subprocess.check_call(['git', 'cherry-pick', commit])
    except subprocess.CalledProcessError:
        # Cancel the cherry-pick if it fails.
        subprocess.check_call(['git', 'cherry-pick', '--quit'])
        # Remove the files that were staged.
        subprocess.check_call(['git', 'reset', '--hard'])
        # Add the failed commit to the list.
        failed_commits.append(commit)
        
# Print a message to the user.
if len(failed_commits) > 0:
    print("\nThe following commit(s) failed to cherry-pick:")
    for commit in failed_commits:
        print(commit)
    print("Please check, manually cherry-picking and resolving conflict as needed.")
